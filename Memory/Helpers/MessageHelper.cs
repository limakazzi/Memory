using System;

namespace Memory.Helpers
{
    //Beautify messages
    public static class MessageHelper
    {
        public static void Warning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(message);
            Console.ResetColor();
        }

        public static void Info(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write(message);
            Console.ResetColor();
        }

        public static void InputRequest(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write(message);
            Console.ResetColor();
        }
        public static void Default(string message)
        {
            Console.ResetColor();
            Console.Write(message);  
        }
        public static void Logo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(message);
            Console.ResetColor();
        }
    }
}
