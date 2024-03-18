using FileProcessing;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.ReplyMarkups;

namespace AeroExpressBot;

public class BotOptions
{
    public const string BadOpenFile = "[Unknown operation in this context]\nUse /openfile command to open a file";

    public const string NoUsername = "Bot can't get access to your username. Check your privacy settings or " +
                                     "add a username to your account";

    private const string StartMessage = "Welcome to Aeroexpress Schedule bot\nThe bot can process CSV and " +
                                        "JSON files that contain information about \nAeroexpresses\n- filter\n" +
                                        "- sort\n- export\n[Вар. 1 Егор Колобаев, БПИ2311]\nUse " +
                                        "/help command to learn how to use it";

    private const string HelpMessage = "/openfile - Open JSON or CSV file\n/filter - Use filtering options\n/sort" +
                                       " - Use " +
                                       "sorting options\n/export - Export file\n/view - View opened file";

    private readonly Manager _manager;

    public BotOptions()
        => throw new ArgumentException("Can't create a bot options instance without a manager");

    public BotOptions(Manager manager)
    {
        _manager = manager;
    }

    /// <summary>
    /// The method handles queries with specifying input based on current user state. 
    /// </summary>
    /// <param name="command">Command input.</param>
    /// <param name="username">User's identifier.</param>
    /// <param name="message">The result that will be displayed to the user.</param>
    /// <param name="keyboardMarkup">The markup that will be displayed to the user.</param>
    /// <returns>true if caller doesn't need to take action.</returns>
    private bool HandleStateInput(string command, string username, out string message, out IReplyMarkup keyboardMarkup)
    {
        keyboardMarkup = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "Open file", "Filter", "Sort" },
            new KeyboardButton[] { "Export", "View", "Help" },
        })
        {
            ResizeKeyboard = true
        };

        _manager.CurrentState.TryAdd(username, Manager.State.Default);

        switch (_manager.CurrentState[username])
        {
            case Manager.State.SpecifyStart:
                try
                {
                    _manager.Filter(Manager.FilterOptions.StationStart, username, command);
                    message = $"Filtered by {command} value";
                }
                catch (Exception e)
                {
                    message = $"There's been an error with filtering: {e.Message}";
                }

                _manager.CurrentState[username] = Manager.State.Default;
                return true;
            case Manager.State.SpecifyEnd:
                try
                {
                    _manager.Filter(Manager.FilterOptions.StationEnd, username, command);
                    message = $"Filtered by {command} value";
                }
                catch (Exception e)
                {
                    message = $"There's been an error with filtering: {e.Message}";
                }

                _manager.CurrentState[username] = Manager.State.Default;
                return true;
            case Manager.State.SpecifyBoth:
                try
                {
                    _manager.Filter(Manager.FilterOptions.Both, username, command);
                    message = $"Filtered by {command} value";
                }
                catch (Exception e)
                {
                    message = $"There's been an error with filtering: {e.Message}";
                }

                _manager.CurrentState[username] = Manager.State.Default;
                return true;
            case Manager.State.Filter:
                switch (command)
                {
                    case "StationStart":
                        message = "Select value for 'StationStart':";
                        _manager.CurrentState[username] = Manager.State.SpecifyStart;
                        keyboardMarkup = new ReplyKeyboardRemove();
                        return true;
                    case "StationEnd":
                        message = "Select value for 'StationEnd':";
                        keyboardMarkup = new ReplyKeyboardRemove();
                        _manager.CurrentState[username] = Manager.State.SpecifyEnd;
                        return true;
                    case "StationStart & StationEnd":
                        message = "Select value for 'StationStart'&'StationEnd' (use '&' sign):";
                        keyboardMarkup = new ReplyKeyboardRemove();
                        _manager.CurrentState[username] = Manager.State.SpecifyBoth;
                        return true;
                    default:
                        message = "Undefined field for filtering";
                        _manager.CurrentState[username] = Manager.State.Default;
                        return false;
                }
            case Manager.State.Sort:
                _manager.CurrentState[username] = Manager.State.Default;
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
            case Manager.State.Export:
                _manager.CurrentState[username] = Manager.State.Default;
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
            case Manager.State.Default:
            default:
                message = "Undefined command";
                return false;
        }
    }

    /// <summary>
    /// The method that handles all the basic commands.
    /// </summary>
    /// <param name="command">Command input.</param>
    /// <param name="username">User's identifier.</param>
    /// <param name="message">The result that will be displayed to the user.</param>
    /// <param name="keyboardMarkup">The markup that will be displayed to the user.</param>
    /// <returns>true if caller doesn't need to take action.</returns>
    public bool HandleBasicCommand(string command, string username, out string message, out IReplyMarkup keyboardMarkup)
    {
        if (HandleStateInput(command, username, out message, out keyboardMarkup)) return true;
        switch (command)
        {
            case "/start":
                message = StartMessage;
                try
                {
                    _manager.ClearUserData(username);
                }
                catch (Exception e)
                {
                    _manager.Logger.LogError($"Couldn't clear the user's data: {e.Message}");
                }

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
                    new KeyboardButton[] { "Export", "View", "Help" },
                })
                {
                    ResizeKeyboard = true
                };
                return true;
            case "/openfile" or "Open another" or "Open file":
                try
                {
                    _manager.ClearUserData(username);
                }
                catch (Exception e)
                {
                    _manager.Logger.LogError($"Couldn't clear the user's data: {e.Message}");
                }

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
                    _manager.CurrentState[username] = Manager.State.Filter;
                    message = "Choose the parameter for filtering";
                    keyboardMarkup = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[] { "StationStart", "StationEnd" },
                        new KeyboardButton[] { "StationStart & StationEnd" }
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
                    _manager.CurrentState[username] = Manager.State.Sort;
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
                        new KeyboardButton[] { "Export", "View", "Help" },
                    })
                    {
                        ResizeKeyboard = true
                    };
                }
                else
                {
                    message = "Choose an export format";
                    _manager.CurrentState[username] = Manager.State.Export;
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
                message = !_manager.TryOpenUserFile(username).Result || !_manager.DataTripsMap.ContainsKey(username)
                    ? "None files are open"
                    : $"{_manager.DataTripsMap[username]}";

                keyboardMarkup = new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton[] { "Open file", "Filter", "Sort" },
                    new KeyboardButton[] { "Export", "View", "Help" },
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