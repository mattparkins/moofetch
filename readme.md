# moofetch

Not-quite-all-purpose data fetcher for json stored on the web:  

- Can continue from where it left off if interrupted;
- Has basic data checks on downloads;
- Can also use downloaded data to direct later downloads*.

(*for instance, downloading leaderboard json, extracting the top 50 entries, and then extracting an id from each entry to then use in later filenames to be downloaded)

Requires .net 6 for the JsonConverter enum support

GenerateConfig generates something like this:

    {
        "dataPath": "/data",
        "skipFetch": false,
        "items": [
            {
                  "uri": "http://bbc.co.uk",
                  "fetchType": "regular",
                  "pageStart": 0,
                  "pageIncrement": 0,
                  "pageCount": 1,
                  "extractPath": "",
                  "loopedItems": null
            }
        ]
    }

The dataPath is required.  For each item to be downloaded, only the uri is required - everything else is related to extracting values from the downloaded json in order to then put those values into uri's to drive the items in loopedItems which is itself a list of items like the items at the root.

For a paged download, all the pages are downloaded and then the extraction takes place on each file with the values placed into a single list, so if there are 50 ids you want to extract on each page, and you instruct the download of 10 pages, there will be a list of 500 ids to be inserted into the uri in loopedItems.

To insert the paging value into the uri use the variable name inside curly braces inside the uri, for instance:

For paging, use page: http://myuri.com/api/page={page}

For extracted values, use value: http://myuri.com/api/userid={value}
