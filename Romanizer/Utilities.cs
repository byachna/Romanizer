namespace Romanizer
{
    public static class Utilities
    {
        /// <summary>
        /// Writes a message to the console in the specified color.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public static void WriteMessage(string message, ConsoleColor color)
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = previousColor;
        }
    }
}