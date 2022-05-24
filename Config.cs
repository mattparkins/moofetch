using System.Collections.Generic;

namespace moofetch {

    public enum FetchType {
        Regular,
        ExtractAndLoop,
        PagedExtractAndLoop,
    }

    public class FetchItem {
        public string uri                   { get; set; } = "";
        public FetchType fetchType          { get; set; } = FetchType.Regular;
        public int pageStart                { get; set; } = 0;
        public int pageIncrement            { get; set; } = 0;
        public int pageCount                { get; set; } = 1;
        public string extractPath           { get; set; } = "";
        public List<FetchItem> loopedItems  { get; set; }
    }

    public class Config {

        public string dataPath              { get; set; }   // relative location for data cache
        public bool skipFetch               { get; set; }   // force the skipping of data fetching - may crash if data isn't in the cache
        public List<FetchItem> items        { get; set; }
        
        public static Config CreateDefault() {
            Config config = new Config();
            config.skipFetch = false;
            config.dataPath = "/data";

            config.items = new List<FetchItem>();
            config.items.Add(new FetchItem { uri = "http://bbc.co.uk" });

            return config;
        }

        public bool IsValid() {
            return (
                dataPath?.Length >=0
                );
        }
    }
}