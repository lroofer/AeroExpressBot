using System.Net;

namespace FileProcessing;

using static Markup;

/// <summary>
/// A custom exception: Can't write this file at some point.
/// </summary>
public class WriteFileException : Exception
{
    public WriteFileException(string message) : base(message)
    {
    }

    public WriteFileException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    public override string ToString()
    {
        return $"Произошла ошибка при записи файла: {Message} в {StackTrace}";
    }
}

/// <summary>
/// A custom exception: impossible to convert data if it doesn't meet the template.
/// </summary>
public class WrongFormatException : Exception
{
    public WrongFormatException(string message) : base(message)
    {
    }

    public override string ToString()
    {
        return $"Произошла ошибка в преобразовании типов: {Message} в {StackTrace}";
    }
}

public static class Manager
{
    public static string? DataFolder;
    public static Dictionary<string, OpenType> Selected = new Dictionary<string, OpenType>();
    public static Dictionary<string, Trips> DataTripsMap = new Dictionary<string, Trips>();
    public const string FORMAT_NAMES =
        "\"Id\";\"StationStart\";\"Line\";\"TimeStart\";\"StationEnd\";\"TimeEnd\";\"global_id\";";

    public const string FORMAT_COLUMNS =
        "\"Локальный идентификатор\";\"Станция отправления\";\"Направление Аэроэкспресс\";\"Время отправления со станции\";\"Конечная станция направления Аэроэкспресс\";\"Время прибытия на конечную станцию направления Аэроэкспресс\";\"global_id\";";

    public static char[] s_separators = { ';', '\"' };

    public enum OpenType
    {
        Closed, OpenCsv, OpenJson
    }

    public enum FilterOptions
    {
        StationStart, StationEnd, Both
    }

    public enum SortOptions
    {
        TimeStart, TimeEnd
    }
    public static void Filter(FilterOptions filterOptions)
    {
        
    }

    public static void Sort(SortOptions sortOptions)
    {
        
    }
    public static bool ProcessFile(string path, string username, out string msg)
    {
        Header(path);
        try
        {
            switch (Path.GetExtension(path))
            {
                case ".csv":
                    var stream = File.OpenRead(path);
                    var csvProcessor = new CsvProcessing();
                    Selected[username] = OpenType.OpenCsv;
                    DataTripsMap[username] = csvProcessor.Read(stream);
                    stream.Close();
                    msg = "Csv file was read";
                    break;
                case ".json":
                    var jsonProcessor = new JsonProcessing();
                    var jsonStream = File.OpenRead(path);
                    Selected[username] = OpenType.OpenJson;
                    DataTripsMap[username] = jsonProcessor.Read(jsonStream);
                    msg = "Json file was read";
                    break;
                default:
                    throw new Exception($"Unknown extension: {Path.GetExtension(path)}");
            }

            return true;
        }
        catch (Exception e)
        {
            msg = e.Message;
            return false;
        }
    }

    public static void ClearUserData(string username)
    {
        var file = GetUserFileName(username, Selected[username] == OpenType.OpenCsv ? "csv" : "json");
        File.Delete(file);
        Selected[username] = OpenType.Closed;
    }

    public static string GetUserFileName(string username, string extenstion) => $"{Path.Join(DataFolder, username)}.{extenstion}";
    
    public static bool TryOpenUserFile(string username)
    {
        if (Selected.ContainsKey(username) && Selected[username] != OpenType.Closed) return true;
        var fileCsv = GetUserFileName(username, "csv");
        if (ProcessFile(fileCsv, username, out var msg1))
        {
            return true;
        }

        var fileJson = GetUserFileName(username, "json");
        return ProcessFile(fileJson, username, out var msg2);
    }
    public static void Init()
    {
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
}