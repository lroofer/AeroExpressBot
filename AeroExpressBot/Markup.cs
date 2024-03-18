namespace AeroExpressBot;

public static class Markup
{
    /// <summary>
    /// The method that prints colored line.
    /// </summary>
    /// <param name="text">Text to print.</param>
    /// <param name="color">Color.</param>
    /// <param name="moveLine">Need to move the carriage to the next line or not.</param>
    private static void PrintColoredLine(string text, ConsoleColor color, bool moveLine = true)
    {
        Console.ForegroundColor = color;
        if (moveLine)
        {
            Console.WriteLine(text);
        }
        else
        {
            Console.Write(text);
        }

        Console.ResetColor();
    }

    /// <summary>
    /// The static method prints colurful headers.
    /// </summary>
    /// <param name="text">Output text.</param>
    /// <param name="moveLine">"true" to move the carriage to the another line.</param>
    public static void Header(string text, bool moveLine = true)
    {
        PrintColoredLine(text, ConsoleColor.Yellow, moveLine);
    }

    /// <summary>
    /// The static method prints colorful warnings/errors.
    /// </summary>
    /// <param name="text">Output text.</param>
    /// <param name="moveLine"></param>
    public static void Warning(string text, bool moveLine = true)
    {
        PrintColoredLine(text, ConsoleColor.Red, moveLine);
    }

    /// <summary>
    /// The static method prints colorful success messages.
    /// </summary>
    /// <param name="text">Output txt.</param>
    /// <param name="moveLine"></param>
    public static void Success(string text, bool moveLine = true)
    {
        PrintColoredLine(text, ConsoleColor.Green, moveLine);
    }
}