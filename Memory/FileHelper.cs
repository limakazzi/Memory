using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Memory
{
    internal class FileHelper
    {
        private string _filePath;

        public FileHelper(string filePath)
        {
            _filePath = filePath;
        }

        public List<string> ReadFromFile()
        {
            using (var reader = new StreamReader(_filePath))
            {
                var words = reader
                    .ReadToEnd()
                    .Split('\n')
                    .Select(x => x.Replace("\r",""))
                    .ToList();

                return words;
            }
        }
    }
}
