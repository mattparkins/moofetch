# moofetch

Moofetch is a downloaded-data driven file fetcher.  Its primary strength is downloading json files, building data collections from entries in that downloaded json file, and then using the data collection to drive further downloads making it exceptionally powerful.

For instance, Moofetch could download the first (say) 4 pages of a leaderboard json file for a fantasy football competition, extract an id from each entry in the leaderboard using JSONPath, and then download the team from each manager in those first 4 pages of the leaderboard using the extracted id.  

- Can continue from where it left off if interrupted;
- Has basic sanity checks on downloads;
- Download rate limiting.

Moofetch does everything through its config file.  Generate the config file using :
	
	dotnet run --generateConfig config.json 

(or copy the example fplconfig.json and customise).  To execute call: 
    
	dotnet run --config config.json 

Here's a walkthrough the example config which uses fantasy premier league (FPL) as its data source.

```json
{
    "dataPath": "data/",
    "callRatePerMin": 0,
    "items": [
        { 
		"uri": "https://fantasy.premierleague.com/api/bootstrap-static/",
		"sanityCheckSize": 100000,
		"output": "bootstrap-static.json",
		"extractCollection": [
			{ "name": "gw",   "path": "$.events[*].id"    },
			{ "name": "elid", "path": "$.elements[*].id"  }
		]
        },
        {
		"uri": "https://fantasy.premierleague.com/api/fixtures/?event={gw}",
		"sanityCheckSize": 100,
		"output": "fixtures_event_{gw}.json"
        },
        {
		"uri": "https://fantasy.premierleague.com/api/element-summary/{elid}/",
		"sanityCheckSize": 100,
		"output": "player_{elid}.json"
        },
        {
		"uri": "https://fantasy.premierleague.com/api/leagues-classic/314/standings/?page_standings={page}&phase=1",
		"sanityCheckSize": 100,
		"output": "overall_standings_page_{page}.json",
		"pageStart": 1,
		"pageIncrement": 1,
		"pageCount": 20,
		"extractCollection": [
			{ "name": "entryid",  "path": "$.standings.results[*].entry" }
		]
        },
        {
		"uri": "https://fantasy.premierleague.com/api/entry/{entryid}/history/",
		"sanityCheckSize": 100,
	        "output": "entry_{entryid}_history.json"
        },
        {
		"uri": "https://fantasy.premierleague.com/api/entry/{entryid}/transfers/",
		"sanityCheckSize": 100,
		"output": "entry_{entryid}_transfers.json"
        },
        {
		"uri": "https://fantasy.premierleague.com/api/entry/{entryid}/event/{gw}/picks/",
		"sanityCheckSize": 100,
		"output": "entry_{entryid}_picks_{gw}.json"
        }
    ]
}
```


**dataPath**, string, required - this is the folder where data is placed.  If a file is requested but it already exists in this dataPath (and it passes the sanity file-length check), it is considered already downloaded.

**callRatePerMin**, int - maximum number of calls to make per minute, or zero for no throttling.

**items**, array - contains an array of objects that describe download tasks.  Each task has the following settings, only the uri is required:

**uri**, string, required, the target to download.  It can include references in curly brackets which will be substituted for a value.  The value can either be a collection name previously extracted (see collection extraction further down), or a reserved reference (presently the only one is 'page' - see paging further down).  If a collection contains more than one value, the uri will be downloaded multiple times with the reference substituted for each value.  If more than one reference is used then all combinations will be called.  For example, as seen with the final task above, a uri containing two references, one with 100 entries and the second with 38 entries, will be called 3800 times.

**sanityCheckSize**, int - the minimum received file size for this file to be considered a valid file.

**output**, string - the filename to be outputted, without this setting, the uri is simply sanitised.  If this item entry downloads more than one file becuase of the references included, ensure you include the reference in the output too, not just the input, else the output file will be overwritten by each download.

**pageStart, pageInterval, PageCount**, int, creates a 'page' reference to be inserted into the uri and output.  'page' starts with a value of pageStart, and after each iteration of this download, pageInterval is added to 'page'.  The loop ceases after pageCount iterations.  The 'page' reference 

**extractCollection**, array - each entry defines a collection to create or append to comprising of its name and a JSONPath.  The downloaded item is loaded and the JSONPath is used to extract data (which is stored as string) and stored in a collection for use in later downloads.  See NewtonSoft's Json.net JSONPath help for more help regarding JSONPath.

---
### Errors

The default use-case is unattended execution, so the program will largely try to plough on through errors where possible and log them to an error.log file in the root folder for the user to peruse at their leisure.

---
Future versions:
- Add sanity deserialization for files ending .json
- Add sanity data checks that include variable referencing (perhaps ensuring that a reference used in the uri is also found in a specified JSONPath)
- Check case sensitivity of various items, particularly references inside uris.
- Asynchronous downloads - initially thought to not be of much use as hammering a single server for data is typically poor form, but it is allowable in some use-cases, particularly if one owns the target server, so perhaps the options should be there?
