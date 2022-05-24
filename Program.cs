using System;
using System.Threading.Tasks;

namespace moofetch {

    static class Program {

        enum Intent {
            DisplayHelp,
            DisplayVersion,
            Execute,
            GenerateConfig
        }

        static readonly int _versionMajor = 0, _versionMinor = 1;

        static void _displayVersion() {
            Console.WriteLine($"moofetch v{_versionMajor}.{_versionMinor}");
        }

        static void _displayHelp() {
            Console.WriteLine("usage: moofetch <option>");
            Console.WriteLine(" --showHelp                          display this help");
            Console.WriteLine(" --showVersion                       display the version number");
            Console.WriteLine(" --config <filename.json>            load configuration and execute");
            Console.WriteLine(" --generateConfig <filename.json>    generate empty configuration file");
        }

        
        static Intent _parseCommandLine(string[] args) {
            Intent intent = Intent.DisplayHelp;

            string sargs = args.Length > 0 ? args[0] : "";
            int minArgCount = 1;

            switch (sargs) {
                case "--showVersion":       intent = Intent.DisplayVersion;                         break;
                case "--config":            intent = Intent.Execute;            minArgCount = 2;    break;
                case "--generateConfig":    intent = Intent.GenerateConfig;     minArgCount = 2;    break;
            }

            if (args.Length < minArgCount) {
                Console.WriteLine("Not enough arguments.");
                return Intent.DisplayHelp;
            }

            return intent;
        }


        static async Task Main(string[] args) {
            
            Intent intent = _parseCommandLine(args);
            switch (intent) {
                case Intent.GenerateConfig:         _generateConfig(args[1]);                   break;
                case Intent.Execute:                await _execute(args[1]);                    break;
                case Intent.DisplayVersion:         _displayVersion();                          break;
                case Intent.DisplayHelp:        
                default:                            _displayHelp();                             break;
            }
        }


        static void _generateConfig(string configFilePath) {
            
            if (Utils.PathIsOutsideFolderStructure(configFilePath)) {
                Console.WriteLine("Cannot write to file outside of this folder or its subfolders.");
                return;
            }

            Console.Write("Creating default config, ");
            Config config = Config.CreateDefault();

            Console.WriteLine($"writing to {configFilePath}");
            Utils.SerializeToFile<Config>(configFilePath, config);
        }


        static Config _loadConfig(string configFilePath) {
            
            Config config;
            Console.Write($"Loading config {configFilePath}, ");
            
            try {
                config = Utils.DeserializeFromFile<Config>(configFilePath);
            } catch {
                config = null;
            } 
            
            if (config == null || !config.IsValid()) {
                Console.WriteLine("invalid or failed to deserialise.");
                Environment.Exit(-1);
            } 

            Console.WriteLine("Ok!");
            return config;
        }


        static async Task _execute(string configFilePath) {
            Config config = _loadConfig(configFilePath);

            //await Engine.Login(config);
        }
    }
}
