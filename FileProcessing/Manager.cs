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
    
    public bool ProcessFile(string path, string username, out string msg)
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

    public void ClearUserData(string username)
    {
        var file = GetUserFileName(username, Selected[username] == OpenType.OpenCsv ? "csv" : "json");
        File.Delete(file);
        Selected[username] = OpenType.Closed;
    }

    public Stream ExportData(string username, string extension)
    {
        throw new NotImplementedException();
    }
    
    public string GetUserFileName(string username, string extenstion) => $"{Path.Join(DataFolder, username)}.{extenstion}";
    
    public bool TryOpenUserFile(string username)
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
}