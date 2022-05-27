using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace moofetch {
    public static class Engine {

        static Dictionary <string, List<string>> _coll;  // Storage for extracting data and then driving later looped downloads
        static Config _config;
        static string _taskString;
        static Regex _regexExtract = new Regex("{(.*?)}");
        static List<string> _reservedIdentifiers = new List<string> {"page", "TaskOutputIndex"};

        public static async Task Execute(Config config) {

            _coll = new Dictionary<string, List<string>>();
            _config = config;

            // Create data folder if it doesn't already exist

            if (!Directory.Exists(config.dataPath)) {
                Console.WriteLine($"Data folder ({config.dataPath}) does not exist, creating.");
                Directory.CreateDirectory(config.dataPath);
            }

            Console.WriteLine($"Found {config.items.Count} task{(config.items.Count == 1 ? "" : "s")} to execute");

            // Execute each fetch task in order

            for (int i = 0; i < config.items.Count; i++) {
                Console.WriteLine($"\nTask {i +1}/{config.items.Count}> Executing, source uri {config.items[i].uri}");
                await _executeTask(i);
            }
        }


        // Find the collection identifiers in a given string (could be uri or ouput string)
        // and return in a list after checking that the collection or reserved variable exists

        private static List<string> _getCollectionIdentifiers(string uri) {

            List<string> ci = new List<string>();

            if (uri != null) {

                MatchCollection mc = _regexExtract.Matches(uri);
                foreach (Match match in mc) {
                    
                    string sid = match.Groups[1].Value;
                    
                    if (!_coll.ContainsKey(sid) && !_reservedIdentifiers.Contains(sid)) {
                        Flog.Log($"Met unexpected identifier ({sid}) in string ({uri}).  Identifiers should either be in a collection or in the reserved word list.  Aborting");
                        Environment.Exit(-1);
                    }

                    ci.Add(sid);
                }
            }

            return ci;
        }


        private static async Task _executeTask(int itemIndex) {
            FetchItem item = _config.items[itemIndex];
            _taskString = $"Task {itemIndex +1}/{_config.items.Count}> ";

            List<string> ci = _getCollectionIdentifiers(item.uri);
            List<string> oi = _getCollectionIdentifiers(item.output);

            // We're going to iterate through every combination of extracted data injected into the uri, track each in ciCounter
            Dictionary<string, int> ciCounter = new Dictionary<string, int>();
            ci.ForEach(identifier => ciCounter.Add(identifier, 0));

            bool taskComplete = false;

            while (!taskComplete) {

                // inject values into the uri

                string uri = item.uri;
                foreach (var (identifier, identifierCounter) in ciCounter) {
                    
                    //Is the identifier user defined or from the reserved list?
                    if (_coll.ContainsKey(identifier)) {

                        uri = Regex.Replace(uri, $"{{{identifier}}}", _coll[identifier][identifierCounter]);

                    } else if (_reservedIdentifiers.Contains(identifier)) {

                        // Todo: handle reserved identifiers, such as paging, or task output indicies

                    }
                }

                // inject values into the output if required

                string outFilename = item.output;
                if (outFilename != null) {
                    oi.ForEach( oid => {

                        if (_coll.ContainsKey(oid)) {

                            int identifierCounter = ciCounter[oid];
                            outFilename = Regex.Replace(outFilename, $"{{{oid}}}", _coll[oid][identifierCounter]);
                        
                        } else {
                            // Todo: handle reserved identifiers, such as paging, or task output indicies
                        }
                    });
                }


                // fetch json

                Console.Write(_taskString);
                string json = await _fetch(uri, item.sanityCheckSize, outFilename);


                // Is there a collection to pull out?
                // If so we need to interrogate the json

                if (item.extractCollection?.Count > 0) {
                    JObject o = JObject.Parse(json);

                    item.extractCollection.ForEach(ec => {

                        if (!_coll.ContainsKey(ec.name)) {
                            _coll.Add(ec.name, new List<string>());
                        }

                        IEnumerable<JToken> tokens = o.SelectTokens(ec.path);
                        foreach (JToken token in tokens) {
                            _coll[ec.name].Add(token.ToString());
                        }
                    });   
                }

                // Do we have collection identifier counters to increment?
                if (ci.Count > 0) {
                    
                    int ciIndex = ci.Count;
                    while (--ciIndex >= 0) {

                        string identifier = ci[ciIndex];
                        ciCounter[identifier]++;
                        if (ciCounter[identifier] >= _coll[identifier].Count) {
                            ciCounter[identifier] = 0;

                            if (ciIndex == 0) {
                                taskComplete = true;                                
                            }
                        } else {
                            break;
                        }
                    }

                } else {

                    // Just a one shot download
                    taskComplete = true;
                }
            }

            // Offer stats on collection extraction only if we've actually extracted collections

            if (item.extractCollection != null) {
                item.extractCollection.ForEach(ec => {
                    Console.WriteLine($"{_taskString}Extracted {ec.name} using {ec.path}, {_coll[ec.name].Count} items extracted.");
                });
            }
        }



        static async Task<string> _fetch(string uri, int minFileSize = 0, string outputFilename = null) {

            // Convert uri to a filename
            string filename = _config.dataPath + (outputFilename == null ? Utils.SanitizeFilename(uri) +".json" : outputFilename);
            string json = "";

            Console.Write($" {uri}, ");

            // If the file exists, and isn't past its expiry then we can try loading a deserialising it
            if (File.Exists(filename)) {

                Console.Write("is cached, ");
                json = File.ReadAllText(filename);

                // If the object isn't null then return it
                if (json.Length > minFileSize) {
                    Console.WriteLine("Looks ok!");
                    return json;
                } else {
                    Console.Write("but file invalid, re-");
                }
            }

            // Either the file doesn't exist, or exists but has expired or didn't deserialize correctly,
            // download and save a fresh copy

            Console.Write("downloading, ");

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage {
                Method = HttpMethod.Get,
                RequestUri = new Uri(uri)
            };
            
            string body;

            try {
                HttpResponseMessage response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                body = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"writing");

                File.WriteAllTextAsync(filename, body).ExecuteConcurrently();
                return body;   

            } catch (Exception e) {

                // Some sort of error, some use cases will cause requests to files that don't exist but won't consider this
                // an error - log it for user awareness and move on.

                Console.WriteLine($"error, logging");
                Flog.Log($"Failed: {uri} : {e.Message}");
            }

            return null;
        }
    }
}