﻿using ConsoleTables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Memory
{
    internal class Game
    {
        private const int ASCII_MinNumberValue = 49;
        private const int ASCII_MaxNumberValue = 52;
        private const int HighscoresMaxAmount = 10;
        private const int ColumnsQuantity = 4;
        private const int ASCII_MinCharValueDifficultyEasy = 65;
        private const int ASCII_MaxCharValueDifficultyEasy = 66;
        private const int RowsQuantityDifficultyEasy = 2;
        private const int WordsAmountDifficultyEasy = 4;
        private const int ChancesAmountDifficultyEasy = 10;
        private const int ASCII_MinCharValueDifficultyHard = 65;
        private const int ASCII_MaxCharValueDifficultyHard = 68;
        private const int RowsQuantityDifficultyHard = 4;
        private const int WordsAmountDifficultyHard = 8;
        private const int ChancesAmountDifficultyHard = 15;
        private const string PartialHighscoreFileName = "_Highscores.txt";

        private static Random s_random = new Random();
        private Stopwatch _timer = new Stopwatch();
        private FileHelper _fileHelper = new FileHelper();
        private ConsoleTable _coveredTable;
        private ConsoleTable _uncoveredTable;

        private int _asciiMinCharValue;
        private int _asciiMaxCharValue;
        private int _timeOfTry;
        private int _wordsLeft;
        private int _chancesLeft;
        private int _wordsAmount;
        private int _chancesAmount;
        private int _rowsQuantity;

        private int[] _firstPick;
        private int[] _secondPick;

        private bool _isSuccess;

        private string _wordsFileName = "Words.txt";
        private string _highscoreFilePath;
        private string _difficultyLevel;
        private string _wordFromCoveredTable;
        private string _wordToCompare;
        private string _firstInput;
        private string _secondInput;

        private List<string> _guessedPicksList = new List<string>();
        private List<string> _allWordsList = new List<string>();

        private void StartGame()
        {
            _timer.Start();

            DrawTable(_coveredTable);

            while (_chancesLeft > 0 && _wordsLeft > 0)
            {
                while (true)
                {
                    MessageHelper.InputRequest("Plese type your first pick (ex. B2): ");
                    _firstInput = Console.ReadLine().ToUpper();

                    if (!IsInputValid(_firstInput))
                        continue;

                    char[] inputChars = _firstInput.ToCharArray();

                    if (!IsInputFromRange(inputChars))
                        continue;

                    var tableValues = Converters.GetUserInputConvertedToIntArray(inputChars);
                    _firstPick = tableValues;
                    UncoverFirstPick(tableValues);
                    break;
                }

                while (true)
                {
                    MessageHelper.InputRequest("Plese type your second pick (ex. B2): ");
                    _secondInput = Console.ReadLine().ToUpper();

                    if(!IsInputValid(_secondInput))
                        continue;

                    if (_secondInput.Equals(_firstInput))
                    {
                        MessageHelper.Warning("You already typed this pick!\n\n");
                        continue;
                    }

                    char[] inputChars = _secondInput.ToCharArray();

                    if (!IsInputFromRange(inputChars))
                        continue;

                    var tableValues = Converters.GetUserInputConvertedToIntArray(inputChars);
                    _secondPick = tableValues;
                    GetWordToCompare(tableValues);
                    break;
                }

                CompareWords(_wordFromCoveredTable, _wordToCompare);
            }

            CheckResult();
            HandleHighscores();
            PrepareNewGame();
        }

        private bool IsInputValid(string input)
        {
            if (input.Length != 2)
            {
                MessageHelper.Warning("Please type the right pick (ex. B2)\n\n");
                return false;
            }
            else if (_guessedPicksList.Contains(input))
            {
                MessageHelper.Warning("You already guessed that pick\n\n");
                return false;
            }
            else return true;
        }

        private bool IsInputFromRange(char[] inputChars)
        {
            if (!Char.IsLetter(inputChars[0]) || 
                !Char.IsDigit(inputChars[1]) || 
                ((int)inputChars[0] < _asciiMinCharValue) || 
                ((int)inputChars[0] > _asciiMaxCharValue) || 
                ((int)inputChars[1] < ASCII_MinNumberValue) || 
                ((int)inputChars[1] > ASCII_MaxNumberValue))
            {
                MessageHelper.Warning("Please type the pick from range\n\n");
                return false;
            }
            else return true;
        }

        private void PrepareNewGame()
        {
            _guessedPicksList.Clear();
            _timer.Reset();
        }

        private void HandleHighscores()
        {
            if (_isSuccess && IsResultAbleToBeHighscore())
            {
                var wantToSaveScore = AskUser("Type yes if you wanna save your score: ");
                if (wantToSaveScore)
                    SaveScore();
            }

            var wantToSeeHighscores = AskUser("\nType yes if you wanna see highscores: ");
            if (wantToSeeHighscores)
                DisplayHighscore();
        }

        private bool IsResultAbleToBeHighscore()
        {
            bool isAble;
            List<Highscore> highscores = GetHighscoreList();

            if (!highscores.Any())
                return true;

            int highscoreAmount = highscores.Count;
            Highscore lastHighscore = highscores.Last();

            if (highscoreAmount < HighscoresMaxAmount)
                isAble = true;
            else if (highscoreAmount == HighscoresMaxAmount && lastHighscore.GuessingTimeInSeconds > _timeOfTry)
            {
                DeleteLastHighscore(lastHighscore, highscores);
                isAble = true;
            }
            else
                isAble = false;

            return isAble;
        }

        private void DeleteLastHighscore(Highscore lastHighscore, List<Highscore> highscores)
        {
            highscores.Remove(lastHighscore);
            _fileHelper.SerializeHighscoresToFile(highscores, _highscoreFilePath);
        }

        private List<Highscore> GetHighscoreList()
        {
            var highscores = _fileHelper.DeserializeHighscoresFromFile(_highscoreFilePath).OrderBy(x => x.GuessingTimeInSeconds).ToList();
            return highscores;
        }

        private void DisplayHighscore()
        {
            ConsoleTable highscoreTable = TableHelper.GetTable(isHighscoreTable: true);
            var highscores = GetHighscoreList();

            foreach (var score in highscores)
            {
                highscoreTable.AddRow(score.Nickname, score.DateOfGame.ToString("dd/MM/yyyy"), score.GuessingTimeInSeconds, score.GuessingTries);
            }

            highscoreTable.Write();
        }

        private void SaveScore()
        {
            MessageHelper.Info("Please type your nickname: ");
            var userNickname = Console.ReadLine();
            var highscores = GetHighscoreList();

            highscores.Add(new Highscore
            {
                Nickname = userNickname,
                DateOfGame = DateTime.Now,
                GuessingTimeInSeconds = _timeOfTry,
                GuessingTries = _chancesAmount - _chancesLeft
            });

            _fileHelper.SerializeHighscoresToFile(highscores, _highscoreFilePath);
        }

        private void CheckResult()
        {
            _timer.Stop();
            TimeSpan timerTime = _timer.Elapsed;
            _timeOfTry = (int) timerTime.TotalSeconds;

            if (_wordsLeft == 0 && _chancesLeft > 0)
            {
                MessageHelper.Info("Congratulations! You win!\n");
                MessageHelper.Info($"It took you {_timeOfTry}s and {_chancesAmount - _chancesLeft} chances to discover {_wordsAmount} words!\n\n");
                _isSuccess = true;
            }
            else
            {
                MessageHelper.Warning("You will win next time!\n");
                MessageHelper.Warning($"You used all {_chancesAmount} chances! {_wordsLeft} words left\n\n");
                _isSuccess = false;
            }
        }

        private void CompareWords(string wordFromCoveredTable, string wordToCompare)
        {
            Uncover(_secondPick);
            DrawTable(_coveredTable);

            if (wordFromCoveredTable.Equals(wordToCompare))
            {
                MessageHelper.Info("Great!");
                Thread.Sleep(2000);
                --_wordsLeft;
                _guessedPicksList.Add(_firstInput);
                _guessedPicksList.Add(_secondInput);
            }

            else
            {
                MessageHelper.Warning("Wrong!");
                Thread.Sleep(2000);
                --_chancesLeft;
                Cover(_firstPick);
                Cover(_secondPick);
            }

            DrawTable(_coveredTable);
        }

        private void Uncover(int[] pickValues)
        {
            _coveredTable.Rows[pickValues[0]].SetValue(_wordToCompare, pickValues[1]);
        }

        private void Cover(int[] pickValues)
        {
            _coveredTable.Rows[pickValues[0]].SetValue("x", pickValues[1]);
        }

        private void GetWordToCompare(int[] tableValues)
        {
            _wordToCompare = _uncoveredTable.Rows[tableValues[0]].GetValue(tableValues[1]).ToString();
        }

        private void UncoverFirstPick(int[] tableValues)
        {

            var wordToUncover = _uncoveredTable.Rows[tableValues[0]].GetValue(tableValues[1]);
            _coveredTable.Rows[tableValues[0]].SetValue(wordToUncover, tableValues[1]);
            _wordFromCoveredTable = wordToUncover.ToString();
            DrawTable(_coveredTable);
        }

        private List<string> GetGameWordsList()
        {
            var i = 0;
            var wordsForGame = new List<string>();

            while (i < _wordsAmount)
            {
                var value = s_random.Next(_allWordsList.Count);
                if (wordsForGame.Contains(_allWordsList[value]))
                    continue;

                wordsForGame.Add(_allWordsList[value]);
                i++;
            }
            wordsForGame.AddRange(wordsForGame);

            return wordsForGame;
        }

        private void PrepareTables(List<string> words)
        {
            _coveredTable = TableHelper.GetTable(_difficultyLevel);
            _uncoveredTable = TableHelper.GetTable(_difficultyLevel);

            for (int i = 0; i < _rowsQuantity; i++)
            {
                for (int j = 1; j <= ColumnsQuantity; j++)
                {
                    int index = s_random.Next(words.Count);
                    var nextWord = words[index];
                    _uncoveredTable.Rows[i].SetValue(nextWord, j);
                    words.RemoveAt(index);
                }
            }
        }

        private void DrawTable(ConsoleTable table)
        {
            Console.Clear();
            MessageHelper.Info($"Difficulty: {_difficultyLevel} \tChances left: {_chancesLeft} \tWords left: {_wordsLeft}\n");
            table.Write();
        }

        public void SetAllWordsList()
        {
            var isFilePathCorrect = false;
            List<string> allWordsList = null;

            do
            {
                var filePath = Path.Combine(Environment.CurrentDirectory, _wordsFileName);

                try
                {
                    allWordsList = _fileHelper.GetWordsListFromFile(filePath);
                    isFilePathCorrect = true;

                }
                catch (FileNotFoundException)
                {
                    MessageHelper.Warning($"File {_wordsFileName} not found!\n");
                    MessageHelper.Warning("Type new filename: ");
                    _wordsFileName = Console.ReadLine();
                }
            } while (!isFilePathCorrect);

            _allWordsList = allWordsList;
        }

        public string GetDifficultyLevel()
        {
            MessageHelper.Default("WELCOME TO MEMORY GAME!\n");
            MessageHelper.Info("For 4 words to discover and 10 chances type: easy\n");
            MessageHelper.Info("For 8 words to discover and 15 chances type: hard\n\n");

            while (true)
            {
                MessageHelper.InputRequest("Please choose difficulty level: ");
                _difficultyLevel = Console.ReadLine().ToLower();

                if (!_difficultyLevel.Equals("easy") && !_difficultyLevel.Equals("hard"))
                {
                    MessageHelper.Warning("Please make sure you typed correctly (easy/hard)!\n\n");
                    continue;
                }

                return _difficultyLevel;
            }
        }

        public void SetUpGame(string difficultyLevel)
        {
            if (difficultyLevel.Equals("easy"))
            {
                _wordsLeft = _wordsAmount = WordsAmountDifficultyEasy;
                _chancesLeft = _chancesAmount = ChancesAmountDifficultyEasy;
                _rowsQuantity = RowsQuantityDifficultyEasy;
                _asciiMinCharValue = ASCII_MinCharValueDifficultyEasy;
                _asciiMaxCharValue = ASCII_MaxCharValueDifficultyEasy;
            }
            else
            {
                _wordsLeft = _wordsAmount = WordsAmountDifficultyHard;
                _chancesLeft = _chancesAmount = ChancesAmountDifficultyHard;
                _rowsQuantity = RowsQuantityDifficultyHard;
                _asciiMinCharValue = ASCII_MinCharValueDifficultyHard;
                _asciiMaxCharValue = ASCII_MaxCharValueDifficultyHard;
            }

            string highscoreFileName = difficultyLevel + PartialHighscoreFileName;
            _highscoreFilePath = Path.Combine(Environment.CurrentDirectory, highscoreFileName);

            List<string> words = GetGameWordsList();
            PrepareTables(words);
            StartGame();
        }

        public bool AskUser(string message)
        {
            MessageHelper.InputRequest(message);
            string userAnswer = Console.ReadLine();
            return userAnswer.Equals("yes");
        }
    }
}
