using System;
using System.IO;
using System.Threading.Tasks;

namespace moofetch {
    public static class Flog {

        static StreamWriter _fout;

        public static async void Log(string output) {
            if (_fout == null) {
                _fout = new StreamWriter("error.log", append: true);
            }

            string dstring = DateTime.Now.ToString("yyyy-MMM-dd HH.mm.ss");
            await _fout.WriteLineAsync($"{dstring} > {output}");
        }
    }
}