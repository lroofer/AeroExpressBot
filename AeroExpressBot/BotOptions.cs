using FileProcessing;
using Telegram.Bot.Types.ReplyMarkups;

namespace AeroExpressBot;

public class BotOptions
{
    private const string StartMessage = "Welcome to Aeroexpress Schedule bot\nThe bot can process CSV and " +
                                        "JSON files that contain information about \nAeroexpresses\n- filter\n" +
                                        "- sort\n- export\n[Вар. 1 Егор Колобаев, БПИ2311]\nUse " +
                                        "/help command to learn how to use it";

    private const string HelpMessage = "/openfile - Open JSON or CSV file\n/filter - Use filtering options\n/sort" +
                                       " - Use " +
                                       "sorting options\n/export - Export file\n/view - View opened file";

    public const string BadOpenFile = "[Unknown operation in this context]\nUse /openfile command to open a file";

    public const string NoUsername = "Bot can't get access to your username. Check your privacy settings or " +
                                     "add a username to your account";

    private readonly Manager _manager;
    private enum State
    {
        Filter, SpecifyStart, SpecifyEnd, SpecifyBoth, Sort, Default, Export
    }

    public BotOptions(Manager manager)
    {
        _manager = manager;
    }
    private State _currentState = State.Default;

    private bool HandleQueries(string command, string username, out string message, out IReplyMarkup keyboardMarkup)
    {
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
            case State.SpecifyStart:
                try
                {
                    _manager.Filter(Manager.FilterOptions.StationStart, username, command);
                    message = $"Filtered by {command} value";
                }
                catch (Exception e)
                {
                    message = $"There's been an error with filtering: {e.Message}";
                }

                _currentState = State.Default;
                return true;
            case State.SpecifyEnd:
                try
                {
                    _manager.Filter(Manager.FilterOptions.StationEnd, username, command);
                    message = $"Filtered by {command} value";
                }
                catch (Exception e)
                {
                    message = $"There's been an error with filtering: {e.Message}";
                }

                _currentState = State.Default;
                return true;
            case State.SpecifyBoth:
                try
                {
                    _manager.Filter(Manager.FilterOptions.Both, username, command);
                    message = $"Filtered by {command} value";
                }
                catch (Exception e)
                {
                    message = $"There's been an error with filtering: {e.Message}";
                }

                _currentState = State.Default;
                return true;
            case State.Filter:
                switch (command)
                {
                    case "StationStart":
                        message = "Select value for 'StationStart':";
                        _currentState = State.SpecifyStart;
                        keyboardMarkup = new ReplyKeyboardRemove();
                        return true;
                    case "StationEnd":
                        message = "Select value for 'StationEnd':";
                        keyboardMarkup = new ReplyKeyboardRemove();
                        _currentState = State.SpecifyEnd;
                        return true;
                    case "StationStart & StationEnd":
                        message = "Select value for 'StationStart'&'StationEnd' (use '&' sign):";
                        keyboardMarkup = new ReplyKeyboardRemove();
                        _currentState = State.SpecifyBoth;
                        return true;
                    default:
                        message = "Undefined field for filtering";
                        _currentState = State.Default;
                        return false;
                }
            case State.Sort:
                _currentState = State.Default;
                switch (command)
                {
                    case "TimeStart (increasing)":
                        _manager.Sort(Manager.SortOptions.TimeStart, username);
                        message = "Sorted by TimeStart increasing";
                        return true;
                    case "TimeEnd(increasing)":
                        _manager.Sort(Manager.SortOptions.TimeEnd, username);
                        message = "Sorted by TimeEnd increasing";
                        return true;
                    default:
                        message = "Undefined field for sorting";
                        return false;
                }
            case State.Export:
                _currentState = State.Default;
                switch (command)
                {
                    case "JSON":
                        message = "Exporting to json";
                        return false;
                    case "CSV":
                        message = "Exporting to csv";
                        return false;
                    default:
                        message = "Undefined format for exporting";
                        return true;
                }
            case State.Default:
            default:
                message = "Undefined command";
                return false;
        }
    }
    
    // true if caller doesn't need to take action.
    public bool HandleCommand(string command, string username, out string message, out IReplyMarkup keyboardMarkup)
    {
        if (HandleQueries(command, username, out message, out keyboardMarkup)) return true;
        switch (command)
        {
            case "/start":
                message = StartMessage;
                _manager.ClearUserData(username);
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
                _manager.ClearUserData(username);
                message = "Send your file";
                keyboardMarkup = new ReplyKeyboardRemove();
                return true;
            case "/filter" or "Filter":
                if (!_manager.TryOpenUserFile(username).Result)
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
                if (!_manager.TryOpenUserFile(username).Result)
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
                if (!_manager.TryOpenUserFile(username).Result)
                {
                    message = "None files are open";
                    keyboardMarkup = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[] { "Open file", "Filter", "Sort" },
                        new KeyboardButton[] {"Export", "View", "Help"},
                    })
                    {
                        ResizeKeyboard = true
                    };
                }
                else
                {
                    message = "Choose an export format";
                    _currentState = State.Export;
                    keyboardMarkup = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[] { "CSV", "JSON" }
                    })
                    {
                        ResizeKeyboard = true
                    };
                }
                
                return true;
            case "/view" or "View":
                if (!_manager.TryOpenUserFile(username).Result)
                {
                    message = "None files are open";
                }
                else
                {
                    message = $"{_manager.DataTripsMap[username]}";
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

        return false;
    }

    public string Error(string msg)
    {
        return $"There's been an error: {msg}";
    }
}