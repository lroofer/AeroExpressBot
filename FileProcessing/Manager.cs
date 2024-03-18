using System.Net;
using Microsoft.Extensions.Logging;

namespace FileProcessing;

using static Markup;

public class Manager
{
    private readonly string _dataFolder;
    private readonly Dictionary<string, OpenType> _selected;
    public readonly Dictionary<string, Trips> DataTripsMap;
    public readonly ILogger Logger;

    public const string FormatNames =
        "\"Id\";\"StationStart\";\"Line\";\"TimeStart\";\"StationEnd\";\"TimeEnd\";\"global_id\";";

    public const string FormatColumns =
        "\"Локальный идентификатор\";\"Станция отправления\";\"Направление Аэроэкспресс\";\"Время отправления со станции\";\"Конечная станция направления Аэроэкспресс\";\"Время прибытия на конечную станцию направления Аэроэкспресс\";\"global_id\";";

    public static readonly char[] SSeparators = { ';', '\"' };

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
        string varFolder;
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
            varFolder = Path.Join(Path.GetFullPath("../../../../"), "var");
            if (!Directory.Exists(varFolder))
            {
                Directory.CreateDirectory(varFolder);
            }

            currentLogName = Path.Join(varFolder,
                $"tmp-{DateTime.Now}.txt");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Couldn't create var directory.\n{e.Message}\n Default was chosen");
            varFolder = "";
            currentLogName = "wrong-format.txt";
        }
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new FileLoggerProvider(currentLogName));
        });
        Logger = loggerFactory.CreateLogger<Manager>();
        Logger.LogInformation($"Bot was started at {DateTime.Now}. Logged to {currentLogName}");
        Console.WriteLine($"Selected data directory: {_dataFolder}");
    }

    public void Filter(FilterOptions filterOptions, string username, string value)
    {
        switch (filterOptions)
        {
            case FilterOptions.StationStart:
                DataTripsMap[username] = new Trips(DataTripsMap[username].Where(u => u.StationStart == value).ToArray());
                break;
            case FilterOptions.StationEnd:
                DataTripsMap[username] = new Trips(DataTripsMap[username].Where(u => u.StationEnd == value).ToArray());
                break;
            case FilterOptions.Both:
                var pars = value.Split('&');
                if (pars.Length != 2) throw new ArgumentException("Two parameters must be given!");
                DataTripsMap[username] = new Trips(DataTripsMap[username].Where(u => u.StationStart == pars[0] && u.StationEnd == pars[1]).ToArray());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(filterOptions), filterOptions, null);
        }
    }

    public void Sort(SortOptions sortOptions, string username)
    {
        DataTripsMap[username].Sort((trip1, trip2) => sortOptions == SortOptions.TimeStart
            ? string.Compare(trip1.TimeStart, trip2.TimeStart, StringComparison.Ordinal)
            : string.Compare(trip1.TimeEnd, trip2.TimeEnd, StringComparison.Ordinal));
    }

    public async Task<(bool, string)> ProcessFile(string path, string username)
    {
        try
        {
            switch (Path.GetExtension(path))
            {
                case ".csv":
                    await using (var stream = File.OpenRead(path))
                    {
                        var csvProcessor = new CsvProcessing();
                        _selected[username] = OpenType.OpenCsv;
                        DataTripsMap[username] = csvProcessor.Read(stream);
                        stream.Close();
                        return (true, "Csv file was read");
                    }
                case ".json":
                    await using (var jsonStream = File.OpenRead(path))
                    {
                        var jsonProcessor = new JsonProcessing();
                        _selected[username] = OpenType.OpenJson;
                        DataTripsMap[username] = await jsonProcessor.Read(jsonStream);
                        jsonStream.Close();
                        return (true, "Json file was read");
                    }
                default:
                    throw new Exception($"Unknown extension: {Path.GetExtension(path)}");
            }
        }
        catch (Exception e)
        {
            return (false, e.Message);
        }
    }

    public void ClearUserData(string username)
    {
        var fileJson = GetUserFileName(username, "json");
        var fileCsv = GetUserFileName(username, "csv");
        _selected[username] = OpenType.Closed;
        if (File.Exists(fileJson)) File.Delete(fileJson);
        if (File.Exists(fileCsv)) File.Delete(fileCsv);
    }

    public Stream ExportData(string username, string extension)
    {
        var file = GetUserFileName(username, extension);
        switch (extension)
        {
            case "json":
                var jsonProcessor = new JsonProcessing();
                return jsonProcessor.Write(DataTripsMap[username], file);
            case "csv":
                var csvProcessor = new CsvProcessing();
                return csvProcessor.Write(DataTripsMap[username], file);
            default:
                throw new ArgumentException($"Wrong format: {extension}");
        }
    }

    public string GetUserFileName(string username, string extenstion) =>
        $"{Path.Join(_dataFolder, username)}.{extenstion}";

    public async Task<bool> TryOpenUserFile(string username)
    {
        if (_selected.ContainsKey(username) && _selected[username] != OpenType.Closed) return true;
        var fileCsv = GetUserFileName(username, "csv");
        var result = await ProcessFile(fileCsv, username);
        if (result.Item1)
        {
            return true;
        }

        var fileJson = GetUserFileName(username, "json");
        var res = await ProcessFile(fileJson, username);
        return res.Item1;
    }
}