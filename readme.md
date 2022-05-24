# moofetch

Not-quite-all-purpose data fetcher for json stored on the web:  

Can continue from where it left off if interrupted;
Has basic data checks on downloads;
Can also use downloaded data to direct later downloads*.

(*for instance, downloading leaderboard json, extracting the top 50 entries, and then extracting an id from each entry to then use in later filenames to be downloaded)