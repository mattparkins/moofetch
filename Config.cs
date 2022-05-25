using System.Collections.Generic;

namespace moofetch {

    public enum FetchType {
        Regular,
        LoopCollection,
        LoopPage
    }

    public class ExtractCollection {
        public string name  { get; set; }
        public string path  { get; set; }
    }

    public class FetchItem {
        public string uri                                   { get; set; } = "";
        public string output                                { get; set; }
        public FetchType fetchType                          { get; set; } = FetchType.Regular;
        public int pageStart                                { get; set; } = 0;
        public int pageIncrement                            { get; set; } = 1;
        public int pageCount                                { get; set; } = 1;
        public List<FetchItem> loopedItems                  { get; set; }
        public List<ExtractCollection> extractCollection    { get; set; }
    }

    public class Config {

        public string dataPath              { get; set; }   // relative location for data cache
        public string baseuri               { get; set; }   // the base URI prepended as-is to each uri
        public bool skipFetch               { get; set; }   // force the skipping of data fetching - may crash if data isn't in the cache
        public List<FetchItem> items        { get; set; }
        
        public static Config CreateDefault() {
            Config config = new Config();
            config.skipFetch = false;
            config.dataPath = "/data";
            config.baseuri = "http://bbc.co.uk/";

            config.items = new List<FetchItem>();
            config.items.Add(new FetchItem { uri = "/myfile.json" });

            return config;
        }

        public bool IsValid() {
            return (
                dataPath?.Length >=0
                );
        }
    }
}