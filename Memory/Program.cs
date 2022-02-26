using System;
using Memory.Helpers;
using System.Diagnostics;

namespace Memory
{
    class Program
    {

        static void Main()
        {
            Game _game = new Game(new Random(), new Stopwatch(), new FileHelper(), "Words.txt");
            bool _playAgain;

            do
            {
                _game.SetAllWordsList();
                _game.ShowStartScreen();
                _game.SetUpGame(_game.GetDifficultyLevel());
                _game.StartGame();
                _playAgain = _game.IsUserWill("Type yes if you would like to play again: ");

            } while (_playAgain);
        }
    }
}
