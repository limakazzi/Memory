using System;

namespace Memory
{
    internal class Program
    {
        private static Game _game = new Game();
        private static bool _playAgain;
        static void Main()
        {
            do
            {
                Console.Clear();
                var difficultyLevel = _game.ChooseDifficultyLevel();
                _game.SetUpGame(difficultyLevel);

                _playAgain = _game.AskUser("\nType yes if you wanna play again: ");
            } while (_playAgain);
        }
    }
}
