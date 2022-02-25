using ConsoleTables;

namespace Memory
{
    public static class TableHelper
    {
        public static ConsoleTable GetTable(string difficultyLevel)
        {
            var table = new ConsoleTable(" ", "1", "2", "3", "4");
            table.Configure(o => o.EnableCount = false);
            table.AddRow("A", "x", "x", "x", "x")
                 .AddRow("B", "x", "x", "x", "x");

            if (difficultyLevel.Equals("hard"))
            {
                table.AddRow("C", "x", "x", "x", "x")
                     .AddRow("D", "x", "x", "x", "x");
            }

            return table;
        }

        public static ConsoleTable GetTable(bool isHighscoreTable)
        {
            var table = new ConsoleTable("Nickname", "Date of game", "Time [s]", "Chances used");
            table.Configure(o => o.EnableCount = false);

            return table;
        }
    }
}
