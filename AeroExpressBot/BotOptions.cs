using FileProcessing;

namespace AeroExpressBot;

public static class BotOptions
{
    private const string StartMessage = "Welcome to Aeroexpress Schedule bot\nThe bot can process CSV and " +
                                        "JSON files that contain information about \nAeroexpresses\n- filter\n" +
                                        "- sort\n- export\n[Вар. 1 Егор Колобаев, БПИ2311]\nUse " +
                                        "/help command to learn how to use it";

    private const string HelpMessage = "/openfile - Open JSON or CSV file\n/filter - Use filtering options\n/sort" +
                                       " - Use " +
                                       "sorting options\n/export - Export file\n/view - View opened file";

    public const string BadOpenFile = "[Unknown operation in this context]\nUse /openfile command to open a file";
    public static bool HandleCommand(string command, out string message)
    {
        switch (command)
        {
            case "/start":
                message = StartMessage;
                return true;
            case "/help":
                message = HelpMessage;
                return true;
            case "/openfile" or "Open another":
                Manager.FileName = null;
                message = "Send your file";
                return true;
            case "/filter" or "Filter":
                message = "None files are open";
                return true;
            case "/sort" or "Sort":
                message = "None files are open";
                return true;
            case "/export" or "Export":
                message = "None files are open";
                return true;
            case "/view" or "View":
                message = "None";
                return true;
            default:
                message = "Undefined command";
                return false;
        }
    }

    public static string Error(string msg)
    {
        return $"There's been an error: {msg}";
    }
}