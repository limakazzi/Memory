using System;

namespace Memory
{
    class Program
    {
        private static Game _game = new Game();
        private static bool _playAgain;
        static void Main()
        {
            do
            {
                Console.Clear();
                string difficultyLevel = _game.GetDifficultyLevel();
                _game.SetUpGame(difficultyLevel);

                _playAgain = _game.AskUser("Type yes if you wanna play again: ");
            } while (_playAgain);
        }
    }
}
