using System.Collections.Generic;

namespace moom {

    public class Config {

        public string apiHost {get; set; }                  // Host if required
        public string apiKey {get; set;}                    // key to access the api
        public string acUser {get; set;}                    // username
        public string acPass {get; set;}                    // password
        public string dataPath {get; set; }                 // relative location for data cache
        public bool skipFetch {get; set;}                   // force the skipping of data fetching - may crash if data isn't in the cache

        
        public static Config CreateDefault() {
            Config config = new Config();
            config.skipFetch = false;
            config.apiKey = "mykey";
            config.apiHost = "myhost";
            config.acUser = "user";
            config.acPass = "pass";
            config.dataPath = "/data";

            return config;
        }

        public bool IsValid() {
            return (
                apiKey?.Length >=0 || 
                apiHost?.Length >=0 || 
                acUser?.Length >=0 ||
                acPass?.Length >=0 ||
                dataPath?.Length >=0
                );
        }
    }
}