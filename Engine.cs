using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http;

namespace moofetch {
    public static class Engine {

        static Dictionary <string, List<string>> _coll;  // Storage for extracting data and then driving later looped downloads
        static Config _config;

        public static async Task Execute(Config config) {

            _coll = new Dictionary<string, List<string>>();
            _config = config;

            // Create data folder if it doesn't already exist

            if (!Directory.Exists(config.dataPath)) {
                Console.WriteLine($"Data folder ({config.dataPath} does not exist, creating.");
                Directory.CreateDirectory(config.dataPath);
            }

            Console.WriteLine($"Found {config.items.Count} task{(config.items.Count == 1 ? "" : "s")} to execute");

            // Execute each fetch task in order

            for (int i = 0; i < config.items.Count; i++) {
                Console.WriteLine($"Executing task {i}: {config.items[i].uri}");
                await _execute(i);

                Environment.Exit(0);
            }
            
        }


        private static async Task _execute(int itemIndex) {
            FetchItem item = _config.items[itemIndex];

            Console.Write($"Task {itemIndex}/{_config.items.Count}> ");
            string json = await _fetch(item.uri, item.sanityCheckSize, item.output);

            // Is there a collection to pull out?
            // If so we need to interrogate the json

            if (item.extractCollection.Count > 0) {
                
            }
        }



        static async Task<string> _fetch(string uri, int minFileSize = 0, string outputFilename = null) {

            // Convert uri to a filename
            string filename = outputFilename == null ? _config.dataPath +Utils.SanitizeFilename(uri) +".json" : outputFilename;
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
                    Console.WriteLine("but file invalid, re-");
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

                Console.Write($"writing");

                File.WriteAllTextAsync(filename, body).ExecuteConcurrently();
                return body;   

            } catch (Exception e) {

                // Some sort of error, some use cases will cause requests to files that don't exist but won't consider this
                // an error - log it for user awareness and move on.

                Console.Write($"error, logging");
                Flog.Log($"Failed: {uri} : {e.Message}");
            }

            return null;
        }
    }
}