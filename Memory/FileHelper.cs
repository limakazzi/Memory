using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Memory
{
    public class FileHelper
    {
        public List<string> GetWordsListFromFile(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            {
                var words = reader
                    .ReadToEnd()
                    .Split('\n')
                    .Select(x => x.Replace("\r",""))
                    .ToList();

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
