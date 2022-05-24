/*using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace leaguedata {

    public static class Fetcher {

        static Config _config;
        static string _dataPath;

        static async public Task<FootballData> FetchData(Config config, string dataPath) {

            FootballData footballData = new FootballData();
            _config = config;
            _dataPath = dataPath;
            _allLeagues = new Dictionary<string, External.League>();

            // Download external league data 

            External.LeaguesQueryResponse leaguesQR = await _fetch<External.LeaguesQueryResponse>("leagues", Utils.DaysInSeconds(7));
            
            _config.bookies.ForEach(bookie => {
                bookie.leagues.ForEach(leaguePath => {

                    External.League league = leaguesQR.response.Find(l => l.CountrySlashLeagueName == leaguePath);
                    
                    if (league == null) {
                        Console.WriteLine($"Failed to find league {leaguePath}, skipping.");
                    } else {
                        _allLeagues[leaguePath] = league;
                    }

                });
            });

            foreach ((string leaguePath, External.League exLeague) in _allLeagues) {

                // Competition comp;

                // // Find the first match using the entire string, or use "area/comp name"

                // if (leaguePath.IndexOf("/") == -1) {
                //     comp = Array.Find<Competition>(competitions.competitions, c => c.name == leaguePath);
                // } else {
                //     string[] leagueSplit = leaguePath.Split("/", 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                //     comp = Array.Find<Competition>(competitions.competitions, c => c.name == leagueSplit?[1] && c.area.name == leagueSplit?[0]);
                // }

                // if (comp == null) {
                //     Console.WriteLine($"Failed to find competition {leaguePath}, skipping.");
                //     continue;
                // }

                // // Create the Competition season to be added to the football data
                // int seasonsAvailable = Math.Min(config.maxSeasonCount, comp.numberOfAvailableSeasons);
                // int yearFinalSeason = comp.currentSeason.startDate.Value.Year;
                // int yearFirstSeason = yearFinalSeason - (seasonsAvailable -1);

                // Console.WriteLine($"{comp.area.name}/{comp.name} found, {comp.numberOfAvailableSeasons} seasons available, fetching {seasonsAvailable} seasons: ");

                // CompetitionSeasons seasons = new CompetitionSeasons();
                // seasons.competition = comp;
                // seasons.startingYear = yearFirstSeason;

                // // Now we have the competition's id we can download the relevant Matches.
                
                // for (int y = yearFirstSeason; y <= yearFinalSeason; y++) {

                //     Console.Write($" - {y}, ");

                //     try {
                //         int cacheExpiresInSeconds = y != yearFinalSeason ? Utils.DaysInSeconds(28) : Utils.HoursInSeconds(3);
                //         Matches matches = await _fetch<Matches>($"competitions/{comp.id}/matches?season={y}", config.apiKey, dataPath, forceUseCache ?  Utils.DaysInSeconds(9999) : cacheExpiresInSeconds);
                //         seasons.seasons.Add(matches);
                //     } catch (Exception) {
                //         Console.WriteLine("not found, skipping");
                //     }
                // }

                // footballData.competitionSeasons.Add(seasons);
            }

            Environment.Exit(0);

            return footballData;
        }


        // Fetch and deserialize a file. First try the local cache - if the file 
        // does not exist in the local cache or it has expired (file stamp + expiry < time now) 
        // then fetch it from the uri and save into the local file cache
        // Expires is an integer denoting seconds from the last write that the file expires, or -1 for never expires

        static async Task<T> _fetch<T>(string uri, int expires) {

            // Convert uri to a filename
            string filename = _dataPath +Utils.SanitizeFilename(uri) +".json";
            string json = "";
            T tret = default(T);
            expires = _config.skipFetch ? Utils.DaysInSeconds(9999) : expires;

            Console.Write($"Retrieving {uri}, ");

            // If the file exists, and isn't past its expiry then we can try loading a deserialising it
            if (File.Exists(filename) && (File.GetLastWriteTimeUtc(filename).AddSeconds(expires) >= DateTime.UtcNow)) {

                Console.Write("from cache, ");
                json = File.ReadAllText(filename);
                tret = JsonSerializer.Deserialize<T>(json, Utils.JSONConfig);

                // If the object isn't null then return it
                if (tret != null) {
                    Console.WriteLine("Ok!");
                    return tret;
                } else {
                    Console.WriteLine("failed, ");
                }
            } else {
                Console.Write("file invalid, "); 
            }

            // Either the file doesn't exist, or exists but has expired or didn't deserialize correctly,
            // download and save a fresh copy

            Console.Write("downloading, ");

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage {
                Method = HttpMethod.Get,


                RequestUri = new Uri("https://"+_config.apiHost+"/v3/" + uri),
                Headers =
                {
                    { "X-RapidAPI-Host", _config.apiHost },
                    { "X-RapidAPI-Key", _config.apiKey },
                },
            };
            
            Console.Write($"requesting over network, ");

            using (var response = await client.SendAsync(request)) {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                tret = JsonSerializer.Deserialize<T>(body, Utils.JSONConfig);

                Console.Write($"writing, ");

                Directory.CreateDirectory(_dataPath);
                File.WriteAllText(filename, body);

                Console.WriteLine($"Ok!");
            }

            return tret;
        }
    }
}*/