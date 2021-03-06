using System.Collections.Generic;

namespace moofetch {

    public class ExtractCollection {
        public string name  { get; set; }
        public string path  { get; set; }

        // Basic sanity checking
        public bool IsValid() {
            return name?.Length > 0 && path?.Length > 0;
        }
    }

    public class AddToCollection {
        public string name  { get; set; }
        public List<string> values  { get; set; }

        // Basic sanity checking
        public bool IsValid() {

            if (name?.Length > 0 && values?.Count > 0) {
                
                bool isValid = true;
                values.ForEach(v => isValid &= v?.Length > 0);
                return isValid;
            }

            return false;
        }
    }

    public class FetchItem {
        public string uri                                   { get; set; } = "";
        public string output                                { get; set; }
        public int sanityCheckSize                          { get; set; }
        public int pageStart                                { get; set; } = 0;
        public int pageIncrement                            { get; set; } = 1;
        public int pageCount                                { get; set; } = 1;
        public List<FetchItem> loopedItems                  { get; set; }
        public List<ExtractCollection> extractCollection    { get; set; }
        public List<AddToCollection> addToCollection        { get; set; }

        // Basic sanity checking
        public bool IsValid() {
            bool listsValid = true;

            if (loopedItems != null) {
                loopedItems.ForEach(li => listsValid &= li.IsValid());
            }

            if (extractCollection != null) {
                extractCollection.ForEach(ec => listsValid &= ec.IsValid());
            }

            if (addToCollection != null) {
                addToCollection.ForEach(ac => listsValid &= ac.IsValid());
            }

            return (uri?.Length > 0 && listsValid);
        }
    }


    public class Config {

        public string dataPath              { get; set; } = ""; // relative location for data cache
        public bool skipFetch               { get; set; }       // force the skipping of data fetching - may crash if data isn't in the cache
        public int callRatePerMin           { get; set; } = 60; // number of API calls per minute, or zero for unlimited
        public List<FetchItem> items        { get; set; }
        
        public static Config CreateDefault() {
            Config config = new Config();
            config.skipFetch = false;
            config.dataPath = "/data";
            config.callRatePerMin = 60;
            
            config.items = new List<FetchItem>();
            config.items.Add(new FetchItem { uri = "/myfile.json" });

            return config;
        }


        // Basic validation check to make sure the config isn't entirely nuts
        public bool IsValid() {

            bool itemsValid = true;

            if (items?.Count > 0) {
                items.ForEach(item => itemsValid &= item.IsValid());
            }

            return (
                dataPath?.Length >=0 && 
                items?.Count > 0 &&
                itemsValid
            );
        }
    }
}