using ConsoleTables;
using Memory.Models.Domains;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Memory.Helpers
{
    internal static class HighscoreHelper
    {
        private const int HighscoresMaxAmount = 10;
        private const string PartialHighscoreFileName = "_Highscores.txt";

        private static FileHelper _fileHelper = new FileHelper();
        private static string _highscoreFileName;

        private static void DeleteLastHighscore(Highscore lastHighscore, List<Highscore> highscores)
        {
            //Delete last element of highscore list
            highscores.Remove(lastHighscore);
            _fileHelper.SerializeHighscoresToFile(highscores, _highscoreFileName);
        }

        private static List<Highscore> GetHighscoreList()
        {
            //Get highscores list ordered by guessing time 
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
            //Generate highscore table and read highscores from file
            ConsoleTable highscoreTable = TableHelper.GetTable(isHighscoreTable: true);
            var highscores = GetHighscoreList();

            //For every highscore add table row
            foreach (var score in highscores)
            {
                highscoreTable.AddRow(score.Nickname, score.DateOfGame.ToString("dd/MM/yyyy"), score.GuessingTimeInSeconds, score.GuessingTries);
            }

            //Display highscore
            highscoreTable.Write();
        }

        public static bool IsResultAbleToBeHighscore(int timeOfTry)
        {
            bool isAble;
            List<Highscore> highscores = GetHighscoreList();

            //If there is no highscores saved you can add your score
            if (!highscores.Any())
                return true;

            int highscoreAmount = highscores.Count;
            Highscore lastHighscore = highscores.Last();

            //If highscore list is not full you can add your score
            //If highscore list is full but your score is better than last highscore, delete it and you can add your score
            //In any other case you cannot save your score
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
