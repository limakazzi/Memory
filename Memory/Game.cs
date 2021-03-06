using ConsoleTables;
using Memory.Helpers;
using Memory.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Memory
{
    internal class Game
    {
        private const int ASCII_MinNumberValue = 49;
        private const int ASCII_MaxNumberValue = 52;
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
        private const string LogoFileName = "Logo.txt";

        private static Random _random;
        private Stopwatch _timer;
        private FileHelper _fileHelper;
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

        private string _wordsFileName;
        private string _difficultyLevel;
        private string _wordFromCoveredTable;
        private string _wordToCompare;
        private string _firstInput;
        private string _secondInput;
        private string CoveredValueLabel;

        private List<string> _guessedPicksList = new List<string>();
        private List<string> _allWordsList;

        public Game(Random random, Stopwatch stopwatch, FileHelper fileHelper, string wordsFileName)
        {
            _random = random;
            _timer = stopwatch;
            _fileHelper = fileHelper;
            _wordsFileName = wordsFileName;
        }

        public void StartGame()
        {
            _timer.Start();

            DisplayTable(_coveredTable);

            while (_chancesLeft > 0 && _wordsLeft > 0)
            {
                while (true)
                {
                    MessageHelper.InputRequest("Plese type your first pick (ex. B2): ");
                    _firstInput = Console.ReadLine().ToUpper();

                    //Input validation
                    if (!IsInputValid(_firstInput))
                        continue;

                    var inputAsCharArray = _firstInput.ToCharArray();

                    //Input validation
                    if (!IsInputFromRange(inputAsCharArray))
                        continue;

                    //Convert input to int array
                    var pickValues = Converters.GetUserInputConvertedToIntArray(inputAsCharArray);
                    _firstPick = pickValues;

                    //Uncover first word based on int arrayy
                    UncoverFirstPick(pickValues);
                    break;
                }

                while (true)
                {
                    MessageHelper.InputRequest("Plese type your second pick (ex. B2): ");
                    _secondInput = Console.ReadLine().ToUpper();

                    //Input validation
                    if (!IsInputValid(_secondInput))
                        continue;

                    //Check if the second input is not the same as the first
                    if (_secondInput.Equals(_firstInput))
                    {
                        MessageHelper.Warning("You already typed this pick!\n\n");
                        continue;
                    }

                    var inputAsCharArray = _secondInput.ToCharArray();

                    //Input validation
                    if (!IsInputFromRange(inputAsCharArray))
                        continue;

                    //Convert input to int array
                    var pickValues = Converters.GetUserInputConvertedToIntArray(inputAsCharArray);
                    _secondPick = pickValues;

                    //Set word to compare (word to uncover) based on int array
                    SetWordToCompare(pickValues);
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
            //Check if input has 2 characters and if is not guessed before
            if (input.Length != 2)
            {
                MessageHelper.Warning("Please type the right pick (ex. B2)\n\n");
                return false;
            }
            else if (_guessedPicksList.Contains(input))
            {
                MessageHelper.Warning("You already guessed that pick!\n\n");
                return false;
            }
            else return true;
        }   

        private bool IsInputFromRange(char[] inputChars)
        {
            //Check if first char is number, second char is letter and range
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
            //Check if user won and if his score could be highscore
            if (_isSuccess && HighscoreHelper.IsResultAbleToBeHighscore(_timeOfTry))
            {
                var wantToSaveScore = IsUserWill("Type yes if you would like to save your score: ");
                if (wantToSaveScore)
                    HighscoreHelper.SaveScore(_timeOfTry, (_chancesAmount - _chancesLeft));
            }

            var wantToSeeHighscores = IsUserWill("\nType yes if you would like to see highscores: ");
            if (wantToSeeHighscores)
                HighscoreHelper.DisplayHighscore();
        }

        private void CheckResult()
        {
            _timer.Stop();
            TimeSpan timerTime = _timer.Elapsed;
            _timeOfTry = (int)timerTime.TotalSeconds;

            //Show message depending of result
            if (_wordsLeft == 0 && _chancesLeft > 0)
            {
                MessageHelper.Info("Congratulations! You win!\n");
                MessageHelper.Info($"It tooks you {_timeOfTry}s and {_chancesAmount - _chancesLeft} chances to discover {_wordsAmount} words!\n\n");
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
            //Uncover second user pick and display table
            Uncover(_secondPick);
            DisplayTable(_coveredTable);

            //If the words match, leave them uncover and decrease the number of remaining words
            if (wordFromCoveredTable.Equals(wordToCompare))
            {
                MessageHelper.Info("Great!");
                Thread.Sleep(2000);
                --_wordsLeft;
                _guessedPicksList.Add(_firstInput);
                _guessedPicksList.Add(_secondInput);
            }

            //If the words not match, cover them again and decrease the number of remaining chances
            else
            {
                MessageHelper.Warning("Wrong!");
                Thread.Sleep(2000);
                --_chancesLeft;
                Cover(_firstPick);
                Cover(_secondPick);
            }

            DisplayTable(_coveredTable);
        }

        private void Uncover(int[] pickValues)
        {
            //Uncover second user pick
            _coveredTable.Rows[pickValues[0]].SetValue(_wordToCompare, pickValues[1]);
        }

        private void Cover(int[] pickValues)
        {
            _coveredTable.Rows[pickValues[0]].SetValue(CoveredValueLabel, pickValues[1]);
        }

        private void SetWordToCompare(int[] tableValues)
        {
            //Depending on converted user second input, get word to compare to first word picked
            _wordToCompare = _uncoveredTable.Rows[tableValues[0]].GetValue(tableValues[1]).ToString();
        }

        private void UncoverFirstPick(int[] tableValues)
        {
            //Depending on converted user first input, from uncovered table, get word to show on covered table and display covered table
            var wordToUncover = _uncoveredTable.Rows[tableValues[0]].GetValue(tableValues[1]);
            _coveredTable.Rows[tableValues[0]].SetValue(wordToUncover, tableValues[1]);
            _wordFromCoveredTable = wordToUncover.ToString();
            DisplayTable(_coveredTable);
        }

        private List<string> GetGameWordsList()
        {
            //From list of all words select amount of words depending of difficulty level
            var i = 0;
            var wordsForGame = new List<string>();

            while (i < _wordsAmount)
            {
                var value = _random.Next(_allWordsList.Count);
                if (wordsForGame.Contains(_allWordsList[value]))
                    continue;

                wordsForGame.Add(_allWordsList[value]);
                i++;
            }
            //Double the game words list
            wordsForGame.AddRange(wordsForGame);

            return wordsForGame;
        }

        private void SetTables(List<string> words)
        {
            //Create value for covering words
            string tableSpacer = new string(' ', (_fileHelper.GetMaxNumOfCharacters()) / 2);
            CoveredValueLabel = tableSpacer + "x" + tableSpacer;

            //Create game tables
            _coveredTable = TableHelper.GetTable(_difficultyLevel, tableSpacer, CoveredValueLabel);
            _uncoveredTable = TableHelper.GetTable(_difficultyLevel, tableSpacer, CoveredValueLabel);

            //Fill uncovered table with words
            for (int i = 0; i < _rowsQuantity; i++)
            {
                for (int j = 1; j <= ColumnsQuantity; j++)
                {
                    int index = _random.Next(words.Count);
                    var nextWord = words[index];
                    _uncoveredTable.Rows[i].SetValue(nextWord, j);
                    words.RemoveAt(index);
                }
            }
        }

        private void DisplayTable(ConsoleTable table)
        {
            Console.Clear();
            MessageHelper.Info($"Difficulty: {_difficultyLevel} \tChances left: {_chancesLeft} \tWords left: {_wordsLeft}\n");
            table.Write();
        }

        public void SetAllWordsList()
        {
            //Try to get list of all words from txt file, catch possible exceptions
            var isFilePathCorrect = false;

            do
            {
                var filePath = Path.Combine(Environment.CurrentDirectory, _wordsFileName);

                try
                {
                    _allWordsList = _fileHelper.GetWordsListFromFile(filePath);
                    isFilePathCorrect = true;
                }
                catch (Exception ex)
                {
                    if (ex is UnauthorizedAccessException)
                        MessageHelper.Warning($"Filename cannot be empty or access to the path is denied!\n");

                    if (ex is FileNotFoundException)
                        MessageHelper.Warning($"File {_wordsFileName} not found!\n");

                    MessageHelper.Warning("Type new filename (ex. words.txt): ");
                    _wordsFileName = Console.ReadLine();
                }

            } while (!isFilePathCorrect);

            Console.Clear();
        }

        public string GetDifficultyLevel()
        {
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

            //Set highscore filepath depending on difficulty level
            HighscoreHelper.SetHighscoreFilePath(difficultyLevel);

            var words = GetGameWordsList();
            SetTables(words);
        }

        public bool IsUserWill(string message)
        {
            MessageHelper.InputRequest(message);
            string userAnswer = Console.ReadLine();
            return userAnswer.Equals("yes");
        }

        public void ShowStartScreen()
        {
            //Try to show game logo, if failed show welcome message
            try
            {
                string filePath = Path.Combine(Environment.CurrentDirectory, LogoFileName);
                string asciiLogo = _fileHelper.GetLogoFromFile(filePath);
                MessageHelper.Logo(asciiLogo);
            }
            catch (Exception)
            {
                MessageHelper.Logo("WELCOME TO MEMORY GAME!\n\n");
            }         
        }
    }
}
