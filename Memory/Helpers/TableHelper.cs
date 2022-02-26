using ConsoleTables;

namespace Memory.Helpers
{
    public static class TableHelper
    {
        public static ConsoleTable GetTable(string difficultyLevel, string tableSpacer, string coveredValueLabel)
        {
            //Generate main game table
            var table = new ConsoleTable(" ", tableSpacer + "1" + tableSpacer,
                                              tableSpacer + "2" + tableSpacer,
                                              tableSpacer + "3" + tableSpacer,
                                              tableSpacer + "4" + tableSpacer
                                        );

            table.Configure(o => o.EnableCount = false);
            table.AddRow("A", coveredValueLabel, coveredValueLabel, coveredValueLabel, coveredValueLabel)
                 .AddRow("B", coveredValueLabel, coveredValueLabel, coveredValueLabel, coveredValueLabel);

            //If hard, add two more rows
            if (difficultyLevel.Equals("hard"))
            {
                table.AddRow("C", coveredValueLabel, coveredValueLabel, coveredValueLabel, coveredValueLabel)
                     .AddRow("D", coveredValueLabel, coveredValueLabel, coveredValueLabel, coveredValueLabel);
            }

            return table;
        }

        public static ConsoleTable GetTable(bool isHighscoreTable)
        {
            //Generate highscores table
            var table = new ConsoleTable("Nickname", "Date of game", "Time [s]", "Chances used");
            table.Configure(o => o.EnableCount = false);

            return table;
        }
    }
}
