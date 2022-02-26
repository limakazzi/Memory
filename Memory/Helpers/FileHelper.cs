using Memory.Models.Domains;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Memory.Helpers
{
    public class FileHelper
    {
        private int _maxNumOfCharacters;

        public int GetMaxNumOfCharacters()
        {
            return _maxNumOfCharacters;
        }
        public string GetLogoFromFile(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            {
                //Read logo from txt file
                string logo = reader.ReadToEnd();
                return logo;
            }
        }

        public List<string> GetWordsListFromFile(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            {
                //Read words from txt file
                List<string> words = reader
                                    .ReadToEnd()
                                    .Split('\n')
                                    .Select(x => x.Replace("\r", ""))
                                    .ToList();

                //Find number of characters in longest word in words list
                _maxNumOfCharacters = 0;
                foreach (string w in words)
                {
                    if (w.Length > _maxNumOfCharacters) _maxNumOfCharacters = w.Length;
                }

                //Center each words in list
                for (int i = 0; i < words.Count; ++i)
                {
                    int toMaxCharacters = _maxNumOfCharacters - words[i].Length;
                    if (toMaxCharacters > 1)
                    {
                        string space = new string(' ', (toMaxCharacters) / 2);
                        words[i] = space + words[i];
                    }
                }

                return words;
            }
        }

        public void SerializeHighscoresToFile(List<Highscore> highscores, string filePath)
        {
            var serializer = new XmlSerializer(typeof(List<Highscore>));

            using (var streamWriter = new StreamWriter(filePath))
            {
                serializer.Serialize(streamWriter, highscores);
                streamWriter.Close();
            }
        }

        public List<Highscore> DeserializeHighscoresFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                return new List<Highscore>();

            var serializer = new XmlSerializer(typeof(List<Highscore>));
            using (var streamReader = new StreamReader(filePath))
            {
                var students = (List<Highscore>)serializer.Deserialize(streamReader);
                streamReader.Close();
                return students;
            }
        }
    }
}
