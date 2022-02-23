using Memory.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Memory
{
    internal class GameLogic
    {
        public static string FilePath =
            Path.Combine(Environment.CurrentDirectory, "Words.txt");

        private static FileHelper _fileHelper = new FileHelper(FilePath);
        private static Random _random = new Random();

        private string _difficultyLevel;
        private int _wordsAmount;
        private int _chancesAmount;
        private int _columnsQuantity;
        private int _rowsQuantity;

        internal string ChooseDifficultyLevel()
        {
            Console.WriteLine("WELCOME TO MEMORY GAME!\n");
            MessageHelper.Info("For 4 words to discover and 10 chances type: easy");
            MessageHelper.Info("For 8 words to discover and 15 chances type: hard\n");

            while (true)
            {
                Console.Write("Please choose difficulty level: ");
                _difficultyLevel = Console.ReadLine().ToLower();

                if (!_difficultyLevel.Equals("easy") && !_difficultyLevel.Equals("hard"))
                {
                    MessageHelper.Warning("Please make sure you typed correctly (easy/hard)!\n");
                    continue;
                }

                return _difficultyLevel;
            }
        }

        internal void SetUpGame(string difficultyLevel)
        {
            if (difficultyLevel.Equals("easy"))
            {
                _wordsAmount = 4;
                _chancesAmount = 10;
                _columnsQuantity = 5;
                _rowsQuantity = 3;
            }
            else
            {
                _wordsAmount = 8;
                _chancesAmount = 15;
                _columnsQuantity = 5;
                _rowsQuantity = 5;
            }

            SetUpGameMatrix();
        }

        private void SetUpGameMatrix()
        {
            //Console.WriteLine($"{_difficultyLevel} \t {_wordsAmount} \t {_chancesAmount} \t {_columnsQuantity} \t {_rowsQuantity}");

            var matrixCells = PrepareMatrixCells();
            PrepareMatrix(matrixCells);
        }

        private List<MatrixCell> PrepareMatrixCells()
        {
            var i = 0;
            var wordsAll = _fileHelper.ReadFromFile();
            var wordsForGame = new List<string>();
            var matrixCells = new List<MatrixCell>();

            while (i < _wordsAmount)
            {
                var value = _random.Next(wordsAll.Count);

                if (wordsForGame.Contains(wordsAll[value]))
                    continue;

                wordsForGame.Add(wordsAll[value]);
                i++;
            }


            foreach (var word in wordsForGame)
            {
                var matrixCell = new MatrixCell
                {
                    DiscoveredValue = word,
                    UndiscoveredValue = "x"
                };

                matrixCells.Add(matrixCell);
            }

            matrixCells.AddRange(matrixCells);

            return matrixCells;
        }

        private void PrepareMatrix(List<MatrixCell> words)
        {
            MatrixCell[,] matrix = new MatrixCell[_rowsQuantity, _columnsQuantity];

            matrix[0, 0] = new MatrixCell { IsHeader = true, HeaderName = " " };
            matrix[0, 1] = new MatrixCell { IsHeader = true, HeaderName = "1" };
            matrix[0, 2] = new MatrixCell { IsHeader = true, HeaderName = "2" };
            matrix[0, 3] = new MatrixCell { IsHeader = true, HeaderName = "3" };
            matrix[0, 4] = new MatrixCell { IsHeader = true, HeaderName = "4" };

            matrix[1, 0] = new MatrixCell { IsHeader = true, HeaderName = "A" };
            matrix[2, 0] = new MatrixCell { IsHeader = true, HeaderName = "B" };

            if (_difficultyLevel.Equals("hard"))
            {
                matrix[3, 0] = new MatrixCell { IsHeader = true, HeaderName = "C" };
                matrix[4, 0] = new MatrixCell { IsHeader = true, HeaderName = "D" };
            }

            for (int i = 1; i < _rowsQuantity; i++)
            {
                for (int j = 1; j < _columnsQuantity; j++)
                {
                    int index = _random.Next(words.Count);
                    var nextWord = words[index];
                    matrix[i, j] = nextWord;
                    words.RemoveAt(index);
                }
            }

            DrawMatrix(matrix);

            Console.WriteLine();
            Console.WriteLine();
        }

        private void DrawMatrix(MatrixCell[,] matrix)
        {
            for (int i = 0; i < _rowsQuantity; i++)
            {
                Console.WriteLine();
                for (int j = 0; j < _columnsQuantity; j++)
                {
                    Console.Write(matrix[i, j].GetName(false) + "\t");
                }
            }
        }
    }
}
