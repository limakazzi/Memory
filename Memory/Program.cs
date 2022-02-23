using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memory
{
    internal class Program
    {
        private static GameLogic _game = new GameLogic();
        static void Main(string[] args)
        {
            while (true)
            {
            var difficultyLevel = _game.ChooseDifficultyLevel();
            _game.SetUpGame(difficultyLevel);
            }
        }
    }
}
