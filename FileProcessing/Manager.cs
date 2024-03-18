using Microsoft.Extensions.Logging;

namespace FileProcessing;

public class Manager
{
    public const string FormatNames =
        "\"Id\";\"StationStart\";\"Line\";\"TimeStart\";\"StationEnd\";\"TimeEnd\";\"global_id\";";

    public const string FormatColumns =
        "\"Локальный идентификатор\";\"Станция отправления\";\"Направление Аэроэкспресс\";\"Время отправления со станции\";\"Конечная станция направления Аэроэкспресс\";\"Время прибытия на конечную станцию направления Аэроэкспресс\";\"global_id\";";

    public static readonly char[] SSeparators = { ';', '\"' };
    public readonly Dictionary<string, State> CurrentState;
    public readonly Dictionary<string, Trips> DataTripsMap;
    public readonly ILogger Logger;
    private readonly string _dataFolder;
    private readonly Dictionary<string, OpenType> _selected;

    /// <summary>
    /// Represents the current user state of specifying information.
    /// Default is equal to None (not specifying).
    /// </summary>
    public enum State
    {
        Filter,
        SpecifyStart,
        SpecifyEnd,
        SpecifyBoth,
        Sort,
        Default,
        Export
    }

    /// <summary>
    /// Stores the information about document for each user.
    /// </summary>
    private enum OpenType
    {
        Closed,
        OpenCsv,
        OpenJson
    }

    public enum FilterOptions
    {
        StationStart,
        StationEnd,
        Both
    }

    public enum SortOptions
    {
        TimeStart,
        TimeEnd
    }

    public Manager()
    {
        string currentLogName;
        _selected = new Dictionary<string, OpenType>();
        DataTripsMap = new Dictionary<string, Trips>();
        try
        {
            _dataFolder = Path.Join(Path.GetFullPath("../../../../"), "data");
            if (!Directory.Exists(_dataFolder))
            {
                Directory.CreateDirectory(_dataFolder);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Couldn't create data directory.\n{e.Message}\n Default was chosen");
            _dataFolder = "";
        }

        try
        {
            var varFolder = Path.Join(Path.GetFullPath("../../../../"), "var");
            if (!Directory.Exists(varFolder))
            {
                Directory.CreateDirectory(varFolder);
            }

            currentLogName = Path.Join(varFolder,
                $"tmp-{DateTime.Now.Day}{DateTime.Now.Month}{DateTime.Now.Year}_{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}.txt");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Couldn't create var directory.\n{e.Message}\n Default was chosen");
            currentLogName = "wrong-format.txt";
        }

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new FileLoggerProvider(currentLogName));
        });
        CurrentState = new Dictionary<string, State>();
        Logger = loggerFactory.CreateLogger<Manager>();
        Logger.LogInformation($"Bot was started at {DateTime.Now}. Logged to {currentLogName}");
        Logger.LogInformation($"Data directory: {_dataFolder}");
        Console.WriteLine($"Selected data directory: {_dataFolder}");
    }

    /// <summary>
    /// Filter user's data by filterOptions.
    /// </summary>
    /// <param name="filterOptions"></param>
    /// <param name="username"></param>
    /// <param name="value">Filtration parameter.</param>
    /// <exception cref="ArgumentException">Value isn't correct.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Unexpected options.</exception>
    public void Filter(FilterOptions filterOptions, string username, string value)
    {
        switch (filterOptions)
        {
            case FilterOptions.StationStart:
                DataTripsMap[username] =
                    new Trips(DataTripsMap[username].Where(u => u.StationStart == value).ToArray());
                break;
            case FilterOptions.StationEnd:
                DataTripsMap[username] = new Trips(DataTripsMap[username].Where(u => u.StationEnd == value).ToArray());
                break;
            case FilterOptions.Both:
                var pars = value.Split('&');
                if (pars.Length != 2) throw new ArgumentException("Two parameters must be given!");
                DataTripsMap[username] = new Trips(DataTripsMap[username]
                    .Where(u => u.StationStart == pars[0] && u.StationEnd == pars[1]).ToArray());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(filterOptions), filterOptions, null);
        }
    }

    /// <summary>
    /// Sort user's data by sortingOptions.
    /// </summary>
    /// <param name="sortOptions"></param>
    /// <param name="username"></param>
    public void Sort(SortOptions sortOptions, string username)
    {
        DataTripsMap[username].Sort((trip1, trip2) => sortOptions == SortOptions.TimeStart
            ? string.Compare(trip1.TimeStart, trip2.TimeStart, StringComparison.Ordinal)
            : string.Compare(trip1.TimeEnd, trip2.TimeEnd, StringComparison.Ordinal));
    }

    /// <summary>
    /// Process local file.
    /// </summary>
    /// <param name="path">Path to local file.</param>
    /// <param name="username">User's identifier.</param>
    /// <exception cref="Exception">Couldn't read data.</exception>
    public async Task ProcessFile(string path, string username)
    {
        switch (Path.GetExtension(path))
        {
            case ".csv":
                await using (var stream = File.OpenRead(path))
                {
                    try
                    {
                        var csvProcessor = new CsvProcessing();
                        _selected[username] = OpenType.OpenCsv;
                        DataTripsMap[username] = csvProcessor.Read(stream);
                        CurrentState[username] = State.Default;
                        stream.Close();
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Data doesn't meet the format: {e.Message}");
                    }


                    return;
                }
            case ".json":
                await using (var jsonStream = File.OpenRead(path))
                {
                    try
                    {
                        var jsonProcessor = new JsonProcessing();
                        DataTripsMap[username] = await jsonProcessor.Read(jsonStream);
                        _selected[username] = OpenType.OpenJson;
                        CurrentState[username] = State.Default;
                        jsonStream.Close();
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Data doesn't meet the format: {e.Message}");
                    }

                    return;
                }
            default:
                throw new Exception($"Unknown exception: {Path.GetExtension(path)}");
        }
    }

    /// <summary>
    /// Set user's records to default and free space.
    /// </summary>
    /// <param name="username">User's identifier.</param>
    public void ClearUserData(string username)
    {
        var fileJson = GetUserFileName(username, "json");
        var fileCsv = GetUserFileName(username, "csv");
        _selected[username] = OpenType.Closed;
        DataTripsMap[username] = new Trips(); 
        CurrentState[username] = State.Default;
        if (File.Exists(fileJson)) File.Delete(fileJson);
        if (File.Exists(fileCsv)) File.Delete(fileCsv);
    }

    /// <summary>
    /// Export data to a local file.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="extension">FileType</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Couldn't be exported</exception>
    public async Task<Stream> ExportData(string username, string extension)
    {
        var file = GetUserFileName(username, extension);
        switch (extension)
        {
            case "json":
                var jsonProcessor = new JsonProcessing();
                return await jsonProcessor.Write(DataTripsMap[username], file);
            case "csv":
                var csvProcessor = new CsvProcessing();
                return await csvProcessor.Write(DataTripsMap[username], file);
            default:
                throw new ArgumentException($"Wrong format: {extension}");
        }
    }

    public string GetUserFileName(string username, string extenstion) =>
        $"{Path.Join(_dataFolder, username)}.{extenstion}";

    public async Task<bool> TryOpenUserFile(string username)
    {
        if (_selected.ContainsKey(username) && _selected[username] != OpenType.Closed && DataTripsMap.ContainsKey(username)) return true;
        try
        {
            var fileCsv = GetUserFileName(username, "csv");
            await ProcessFile(fileCsv, username);
            return true;
        }
        catch (Exception e)
        {
            Logger.LogInformation(e.Message);
        }

        try
        {
            var fileJson = GetUserFileName(username, "json");
            await ProcessFile(fileJson, username);
            return true;
        }
        catch (Exception e)
        {
            Logger.LogInformation(e.Message);
        }

        return false;
    }
}