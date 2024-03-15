using FileProcessing;

namespace AeroExpressBot;
using static Markup;

internal static class Program
{
    private static void Main()
    {
        do
        {
            try
            {
                var bot = new Bot();
                Header("Starting bot manager...");
                _ = Task.Run(() => bot.StartBot());
                Header("Press Q to exit the bot");
                while (Console.ReadKey(true).Key != ConsoleKey.Q){}
                return;
            }
            catch (Exception e)
            {
                Warning($"There's been an error with starting the bot manager:\n{e.Message}");
                Header("Press Y to reload, any other key to exit");
                if (Console.ReadKey(true).Key != ConsoleKey.Y) return;
            }
            
        } while (true);
    }
}