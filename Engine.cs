using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace moofetch {
    public static class Engine {
        public static async Task Login(Config config) {

            SessionAPI.Request sr = new SessionAPI.Request { 
                identifier = config.acUser, 
                password = config.acPass
            };

            StringContent scontent = new StringContent(JsonSerializer.Serialize(sr, Utils.JSONConfig));
            scontent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            scontent.Headers.Add("version","2");
            scontent.Headers.Add("X-IG-API-KEY", config.apiKey);

            HttpClient client = new HttpClient();
            
            using (var response = await client.PostAsync(config.apiHost+"/session", scontent)) {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();

                Console.Write($"writing, ");
                Directory.CreateDirectory(config.dataPath);
                File.WriteAllText(config.dataPath+"/response.json", body);

                var tret = JsonSerializer.Deserialize<SessionAPI.Response>(body, Utils.JSONConfig);

                Console.WriteLine($"Ok!");
            }
        }
    }
}