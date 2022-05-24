using System.Collections.Generic;

namespace moofetch {
    public enum FetchType {
        Regular,
        ExtractAndLoop,
    }

    public class FetchItem {
        public string uri                   { get; set; }
        public FetchType fetchType          { get; set; }
        public string extractRegex          { get; set; }
        public List<FetchItem> loopedItems  { get; set; }
    }

    public class Config {

        public string dataPath          { get; set; }   // relative location for data cache
        public bool skipFetch           { get; set; }   // force the skipping of data fetching - may crash if data isn't in the cache
        public List<FetchItem> items    { get; set; }
        
        public static Config CreateDefault() {
            Config config = new Config();
            config.skipFetch = false;
            config.dataPath = "/data";
            config.items = new List<FetchItem>();
            config.items.Add(new FetchItem {"http://bbc.co.uk"});

            return config;
        }

        public bool IsValid() {
            return (
                dataPath?.Length >=0
                );
        }
    }
}