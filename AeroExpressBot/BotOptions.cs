using FileProcessing;
using Telegram.Bot.Types.ReplyMarkups;

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

    public enum State
    {
        Filter, Sort, Default
    }

    private static State _currentState = State.Default;
    public static bool HandleCommand(string command, out string message, out IReplyMarkup keyboardMarkup)
    {
        switch (command)
        {
            case "/start":
                message = StartMessage;
                Manager.FileName = null;
                keyboardMarkup = new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton[] { "Open file", "Help" },
                })
                {
                    ResizeKeyboard = true
                };
                return true;
            case "/help" or "Help":
                message = HelpMessage;
                keyboardMarkup = new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton[] { "Open file", "Filter", "Sort" },
                    new KeyboardButton[] {"Export", "View", "Help"},
                })
                {
                    ResizeKeyboard = true
                };
                return true;
            case "/openfile" or "Open another" or "Open file":
                Manager.FileName = null;
                message = "Send your file";
                keyboardMarkup = new ReplyKeyboardRemove();
                return true;
            case "/filter" or "Filter":
                if (Manager.FileName == null)
                {
                    keyboardMarkup = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[] { "Open file", "Help" },
                    })
                    {
                        ResizeKeyboard = true
                    };
                    message = "None files are open";
                }
                else
                {
                    _currentState = State.Filter;
                    message = "Choose the parameter for filtering";
                    keyboardMarkup = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[] { "StationStart", "StationEnd" },
                        new KeyboardButton[] {"StationStart & StationEnd"}
                    })
                    {
                        ResizeKeyboard = true
                    };
                }

                return true;
            case "/sort" or "Sort":
                if (Manager.FileName == null)
                {
                    keyboardMarkup = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[] { "Open file", "Help" },
                    })
                    {
                        ResizeKeyboard = true
                    };
                    message = "None files are open";
                }
                else
                {
                    _currentState = State.Sort;
                    message = "Choose the parameter for sorting";
                    keyboardMarkup = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[] { "TimeStart (increasing)", "TimeEnd(increasing)" },
                    })
                    {
                        ResizeKeyboard = true
                    };
                }

                return true;
            case "/export" or "Export":
                if (Manager.FileName == null)
                {
                    message = "None files are open";
                }
                else
                {
                    message = "Exporting";
                    // TODO: Export file
                }
                keyboardMarkup = new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton[] { "Open file", "Filter", "Sort" },
                    new KeyboardButton[] {"Export", "View", "Help"},
                })
                {
                    ResizeKeyboard = true
                };
                return true;
            case "/view" or "View":
                if (Manager.FileName == null)
                {
                    message = "None files are open";
                }
                else
                {
                    message = $"{Manager.MTrips?.Count}";
                    // TODO: View file
                }
                keyboardMarkup = new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton[] { "Open file", "Filter", "Sort" },
                    new KeyboardButton[] {"Export", "View", "Help"},
                })
                {
                    ResizeKeyboard = true
                };
                return true;
        }

        keyboardMarkup = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "Open file", "Filter", "Sort" },
            new KeyboardButton[] {"Export", "View", "Help"},
        })
        {
            ResizeKeyboard = true
        };
        
        switch (_currentState)
        {
            case State.Filter:
                _currentState = State.Default;
                switch (command)
                {
                    case "StationStart":
                        Manager.Filter(Manager.FilterOptions.StationStart);
                        message = "Filtered by StationStart value";
                        return true;
                    case "StationEnd":
                        Manager.Filter(Manager.FilterOptions.StationEnd);
                        message = "Filtered by StationEnd value";
                        return true;
                    case "StationStart & StationEnd":
                        Manager.Filter(Manager.FilterOptions.Both);
                        message = "Filtered by both StationStart & StationEnd";
                        return true;
                    default:
                        message = "Undefined field for filtering";
                        return false;
                }
            case State.Sort:
                _currentState = State.Default;
                switch (command)
                {
                    case "TimeStart (increasing)":
                        Manager.Sort(Manager.SortOptions.TimeStart);
                        message = "Sorted by TimeStart increasing";
                        return true;
                    case "TimeEnd(increasing)":
                        Manager.Sort(Manager.SortOptions.TimeEnd);
                        message = "Sorted by TimeEnd increasing";
                        return true;
                    default:
                        message = "Undefined field for sorting";
                        return false;
                }
            case State.Default:
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