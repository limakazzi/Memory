using ConsoleTables;
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
        private const int Highscores_Max_Amount = 10;
        private const int ASCII_NumberValue_Min = 49;
        private const int ASCII_NumberValue_Max = 52;

        private int _ASCII_CharValue_Min;
        private int _ASCII_CharValue_Max;

        private bool _isSuccess;

        private static Random _random = new Random();

        private Stopwatch _timer = new Stopwatch();
        private int _timeOfTry;

        private ConsoleTable _coveredTable;
        private ConsoleTable _uncoveredTable;

        private string _highscoreFilePath;
        private FileHelper _fileHelper = new FileHelper();

        private string _difficultyLevel;
        private int _wordsLeft;
        private int _chancesLeft;
        private int _wordsAmount;
        private int _chancesAmount;
        private int _columnsQuantity;
        private int _rowsQuantity;

        private string _wordFromCoveredTable;
        private string _wordToCompare;

        private int[] _firstPick;
        private int[] _secondPick;

        private string _firstInput;
        private string _secondInput;

        private List<string> _guessedPicks = new List<string>();

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

                    if (_firstInput.Length != 2)
                    {
                        MessageHelper.Warning("Please type the right pick (ex. B2)\n\n");
                        continue;
                    }

                    if (_guessedPicks.Contains(_firstInput))
                    {
                        MessageHelper.Warning("You already guessed that pick\n\n");
                        continue;
                    }

                    var chars = _firstInput.ToCharArray();

                    if (!Char.IsLetter(chars[0])
                       || !Char.IsDigit(chars[1])
                       || (int)chars[0] < _ASCII_CharValue_Min
                       || (int)chars[0] > _ASCII_CharValue_Max
                       || (int)chars[1] < ASCII_NumberValue_Min
                       || (int)chars[1] > ASCII_NumberValue_Max)
                    {
                        MessageHelper.Warning("Please type the pick from range\n\n");
                        continue;
                    }

                    var tableValues = GetValueFromUncoveredTable(chars);
                    _firstPick = tableValues;
                    UncoverFirstPick(tableValues);
                    break;
                }

                while (true)
                {
                    MessageHelper.InputRequest("Plese type your second pick (ex. B2): ");
                    _secondInput = Console.ReadLine().ToUpper();

                    if (_secondInput.Length != 2)
                    {
                        MessageHelper.Warning("Please type the right pick (ex. B2)\n\n");
                        continue;
                    }

                    if (_guessedPicks.Contains(_secondInput))
                    {
                        MessageHelper.Warning("You already guessed that pick\n\n");
                        continue;
                    }

                    else if (_secondInput.Equals(_firstInput))
                    {
                        MessageHelper.Warning("You already typed this pick!\n\n");
                        continue;
                    }

                    var chars = _secondInput.ToCharArray();

                    if (!Char.IsLetter(chars[0])
                       || !Char.IsDigit(chars[1])
                       || (int)chars[0] < _ASCII_CharValue_Min
                       || (int)chars[0] > _ASCII_CharValue_Max
                       || (int)chars[1] < ASCII_NumberValue_Min
                       || (int)chars[1] > ASCII_NumberValue_Max)
                    {
                        MessageHelper.Warning("Please type the pick from range\n\n");
                        continue;
                    }

                    var tableValues = GetValueFromUncoveredTable(chars);
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

        private void PrepareNewGame()
        {
            _guessedPicks.Clear();
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

            if (highscoreAmount < Highscores_Max_Amount)
                isAble = true;
            else if (highscoreAmount == Highscores_Max_Amount && lastHighscore.GuessingTimeInSeconds > _timeOfTry)
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
            _fileHelper.SerializeToFile(highscores, _highscoreFilePath);
        }

        private List<Highscore> GetHighscoreList()
        {
            var highscores = _fileHelper.DeserializeFromFile(_highscoreFilePath).OrderBy(x => x.GuessingTimeInSeconds).ToList();
            return highscores;
        }

        private void DisplayHighscore()
        {
            ConsoleTable highscoreTable = GenerateTable(isHighscoreTable: true);
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

            _fileHelper.SerializeToFile(highscores, _highscoreFilePath);
        }

        private void CheckResult()
        {
            _timer.Stop();
            TimeSpan _timerTime = _timer.Elapsed;
            _timeOfTry = (int) _timerTime.TotalSeconds;

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
                _guessedPicks.Add(_firstInput);
                _guessedPicks.Add(_secondInput);
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

        private int[] GetValueFromUncoveredTable(char[] chars)
        {
            var pickedRow = 0;
            var pickedColumn = 1;

            switch (chars[0])
            {
                case 'A':
                    pickedRow = 0;
                    break;
                case 'B':
                    pickedRow = 1;
                    break;
                case 'C':
                    pickedRow = 2;
                    break;
                case 'D':
                    pickedRow = 3;
                    break;
                default: break;
            }

            switch (chars[1])
            {
                case '1':
                    pickedColumn = 1;
                    break;
                case '2':
                    pickedColumn = 2;
                    break;
                case '3':
                    pickedColumn = 3;
                    break;
                case '4':
                    pickedColumn = 4;
                    break;
                default: break;
            }

            int[] tableValues = new int[] { pickedRow, pickedColumn };

            return tableValues;

        }

        private List<string> PrepareWords()
        {
            var i = 0;

            string wordsFileName = "Words.txt";     //Przenieś do zakresu globanego, albo stworzyć osobny plik z ustawieniami

            List<string> wordsAll = null;
            bool isFilePathCorrect = false;

            do
            {
                var filePath = Path.Combine(Environment.CurrentDirectory, wordsFileName);

                try
                {
                    wordsAll = _fileHelper.ReadFromFile(filePath);
                    isFilePathCorrect = true;
                }
                catch (FileNotFoundException)
                {
                    MessageHelper.Warning($"File {wordsFileName} not found!\n");
                    MessageHelper.Warning("Type new filename: ");
                    wordsFileName = Console.ReadLine();
                }
            } while (!isFilePathCorrect);


            var wordsForGame = new List<string>();

            while (i < _wordsLeft)
            {
                var value = _random.Next(wordsAll.Count);

                if (wordsForGame.Contains(wordsAll[value]))
                    continue;

                wordsForGame.Add(wordsAll[value]);
                i++;
            }

            wordsForGame.AddRange(wordsForGame);

            return wordsForGame;
        }

        private void PrepareTables(List<string> words)
        {
            _coveredTable = GenerateTable();
            _uncoveredTable = GenerateTable();

            for (int i = 0; i < _rowsQuantity; i++)
            {
                for (int j = 1; j < _columnsQuantity; j++)
                {
                    int index = _random.Next(words.Count);
                    var nextWord = words[index];
                    _uncoveredTable.Rows[i].SetValue(nextWord, j);
                    words.RemoveAt(index);
                }
            }

            Console.WriteLine();
        }

        private void DrawTable(ConsoleTable table)
        {
            Console.Clear();
            MessageHelper.Info($"Difficulty: {_difficultyLevel} \tChances left: {_chancesLeft} \tWords left: {_wordsLeft}\n");
            table.Write();
        }
        private ConsoleTable GenerateTable()
        {
            var table = new ConsoleTable(" ", "1", "2", "3", "4");
            table.Configure(o => o.EnableCount = false);
            table.AddRow("A", "x", "x", "x", "x")
                 .AddRow("B", "x", "x", "x", "x");

            if (_difficultyLevel.Equals("hard"))
            {
                table.AddRow("C", "x", "x", "x", "x")
                     .AddRow("D", "x", "x", "x", "x");
            }

            return table;
        }
        private ConsoleTable GenerateTable(bool isHighscoreTable)
        {
            var table = new ConsoleTable("Nickname", "Date of game", "Time [s]", "Chances used");
            table.Configure(o => o.EnableCount = false);

            return table;
        }

        public string ChooseDifficultyLevel()
        {
            Console.WriteLine("WELCOME TO MEMORY GAME!\n");
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
                _wordsLeft = _wordsAmount = 4;
                _chancesLeft = _chancesAmount = 10;
                _columnsQuantity = 5;
                _rowsQuantity = 2;

                _ASCII_CharValue_Min = 65;
                _ASCII_CharValue_Max = 66;
            }
            else
            {
                _wordsLeft = _wordsAmount = 8;
                _chancesLeft = _chancesAmount = 15;
                _columnsQuantity = 5;
                _rowsQuantity = 4;

                _ASCII_CharValue_Min = 65;
                _ASCII_CharValue_Max = 68;
            }

            string highscoreFileName = difficultyLevel + "_Highscores.txt";
            _highscoreFilePath = Path.Combine(Environment.CurrentDirectory, highscoreFileName);

            var words = PrepareWords();
            PrepareTables(words);
            StartGame();
        }
        public bool AskUser(string message)
        {
            MessageHelper.InputRequest(message);
            var userAnswer = Console.ReadLine();
            return userAnswer.Equals("yes");
        }
    }
}
