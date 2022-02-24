using System;

namespace Memory
{
    public class Highscore
    {
        public string Nickname { get; set; }
        public DateTime DateOfGame { get; set; }
        public int GuessingTimeInSeconds { get; set; }
        public int GuessingTries { get; set; }
    }
}
