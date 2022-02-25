using ConsoleTables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Memory
{
    internal static class HighscoreHelper
    {
        private const int HighscoresMaxAmount = 10;
        private const string PartialHighscoreFileName = "_Highscores.txt";

        private static FileHelper _fileHelper = new FileHelper();
        private static string _highscoreFileName;

        private static void DeleteLastHighscore(Highscore lastHighscore, List<Highscore> highscores)
        {
            highscores.Remove(lastHighscore);
            _fileHelper.SerializeHighscoresToFile(highscores, _highscoreFileName);
        }

        private static List<Highscore> GetHighscoreList()
        {
            var highscores = _fileHelper.DeserializeHighscoresFromFile(_highscoreFileName).OrderBy(x => x.GuessingTimeInSeconds).ToList();
            return highscores;
        }

        public static void SetHighscoreFilePath(string partialName)
        {
            string highscoreFileName = partialName + PartialHighscoreFileName;
            _highscoreFileName = Path.Combine(Environment.CurrentDirectory, highscoreFileName);
        }

        public static void SaveScore(int timeOfTry, int chances)
        {
            MessageHelper.Info("Please type your nickname: ");
            var userNickname = Console.ReadLine();
            var highscores = GetHighscoreList();

            highscores.Add(new Highscore
            {
                Nickname = userNickname,
                DateOfGame = DateTime.Now,
                GuessingTimeInSeconds = timeOfTry,
                GuessingTries = chances
            });

            _fileHelper.SerializeHighscoresToFile(highscores, _highscoreFileName);
        }

        public static void DisplayHighscore()
        {
            ConsoleTable highscoreTable = TableHelper.GetTable(isHighscoreTable: true);
            var highscores = GetHighscoreList();

            foreach (var score in highscores)
            {
                highscoreTable.AddRow(score.Nickname, score.DateOfGame.ToString("dd/MM/yyyy"), score.GuessingTimeInSeconds, score.GuessingTries);
            }

            highscoreTable.Write();
        }

        public static bool IsResultAbleToBeHighscore(int timeOfTry)
        {
            bool isAble;
            List<Highscore> highscores = GetHighscoreList();

            if (!highscores.Any())
                return true;

            int highscoreAmount = highscores.Count;
            Highscore lastHighscore = highscores.Last();

            if (highscoreAmount < HighscoresMaxAmount)
                isAble = true;
            else if (highscoreAmount == HighscoresMaxAmount && lastHighscore.GuessingTimeInSeconds > timeOfTry)
            {
                DeleteLastHighscore(lastHighscore, highscores);
                isAble = true;
            }
            else
                isAble = false;

            return isAble;
        }
    }
}
