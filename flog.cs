using System;
using System.IO;
using System.Threading.Tasks;

namespace moofetch {
    public static class Flog {

        static StreamWriter _fout;

        public static async void LogAsync(string output, bool showOnConsole = false) {
            
            string sout = _prepareToLog(output, showOnConsole);
            await _fout.WriteLineAsync(sout);
            await _fout.FlushAsync();
        }


        public static void Log(string output, bool showOnConsole = false) {

            string sout = _prepareToLog(output, showOnConsole);
            _fout.WriteLine(sout);
            _fout.Flush();
        }


        private static string _prepareToLog(string output, bool showOnConsole) {
            if (_fout == null) {
                _fout = new StreamWriter("error.log", append: true);
            }

            string dstring = DateTime.Now.ToString("yyyy-MMM-dd HH.mm.ss");
            string sout = $"{dstring} > {output}";

            if (showOnConsole) {
                Console.WriteLine(sout);
            }

            return sout;
        }
    }
}