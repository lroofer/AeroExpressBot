using System.Net;

namespace FileProcessing;

using static Markup;

public class Manager
{
    public string DataFolder;
    public Dictionary<string, OpenType> Selected;
    public Dictionary<string, Trips> DataTripsMap;

    public const string FormatNames =
        "\"Id\";\"StationStart\";\"Line\";\"TimeStart\";\"StationEnd\";\"TimeEnd\";\"global_id\";";

    public const string FormatColumns =
        "\"Локальный идентификатор\";\"Станция отправления\";\"Направление Аэроэкспресс\";\"Время отправления со станции\";\"Конечная станция направления Аэроэкспресс\";\"Время прибытия на конечную станцию направления Аэроэкспресс\";\"global_id\";";

    public static char[] s_separators = { ';', '\"' };

    public enum OpenType
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
        Selected = new Dictionary<string, OpenType>();
        DataTripsMap = new Dictionary<string, Trips>();
        try
        {
            DataFolder = Path.Join(Path.GetFullPath("../../../../"), "data");
            if (!Directory.Exists(DataFolder))
            {
                Directory.CreateDirectory(DataFolder);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Couldn't create data directory.\n{e.Message}\n Default was chosen");
            DataFolder = "";
        }

        Console.WriteLine($"Selected data directory: {DataFolder}");
    }

    public void Filter(FilterOptions filterOptions)
    {
    }

    public void Sort(SortOptions sortOptions)
    {
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
                        Selected[username] = OpenType.OpenCsv;
                        DataTripsMap[username] = csvProcessor.Read(stream);
                        stream.Close();
                        return (true, "Csv file was read");
                    }
                case ".json":
                    await using (var jsonStream = File.OpenRead(path))
                    {
                        var jsonProcessor = new JsonProcessing();
                        Selected[username] = OpenType.OpenJson;
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
        Selected[username] = OpenType.Closed;
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
        $"{Path.Join(DataFolder, username)}.{extenstion}";

    public async Task<bool> TryOpenUserFile(string username)
    {
        if (Selected.ContainsKey(username) && Selected[username] != OpenType.Closed) return true;
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