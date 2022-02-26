namespace Memory.Models
{
    public static class Converters
    {
        //Convert user input (string as char array) to int array
        //Int array is used for select corresponding table rows and columns
        public static int[] GetUserInputConvertedToIntArray(char[] input)
        {
            int selectedRow = 0;
            int selectedColumn = 1;

            switch (input[0]) {
                case 'B':
                    selectedRow = 1;
                    break;
                case 'C':
                    selectedRow = 2;
                    break;
                case 'D':
                    selectedRow = 3;
                    break;
                default: break;
            }

            switch (input[1]) {
                case '2':
                    selectedColumn = 2;
                    break;
                case '3':
                    selectedColumn = 3;
                    break;
                case '4':
                    selectedColumn = 4;
                    break;
                default: break;
            }

            int[] convertedInput = new int[] { selectedRow, selectedColumn };

            return convertedInput;
        }
    }
}
