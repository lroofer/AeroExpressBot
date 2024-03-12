using System.Net;

namespace FileProcessing;

using static Markup;

/// <summary>
/// A custom exception: Can't write this file at some point.
/// </summary>
public class WriteFileException : Exception
{
    public WriteFileException(string message): base(message) {}
    public WriteFileException(string message, Exception? innerException) : base(message, innerException) {}
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
    public WrongFormatException(string message): base(message) {}
    public override string ToString()
    {
        return $"Произошла ошибка в преобразовании типов: {Message} в {StackTrace}";
    }
}

public static class Manager
{
    public static string? DataFolder;
    public static string? FileName;

    public const string FORMAT_NAMES =
        "\"Id\";\"StationStart\";\"Line\";\"TimeStart\";\"StationEnd\";\"TimeEnd\";\"global_id\";";

    public const string FORMAT_COLUMNS =
        "\"Локальный идентификатор\";\"Станция отправления\";\"Направление Аэроэкспресс\";\"Время отправления со станции\";\"Конечная станция направления Аэроэкспресс\";\"Время прибытия на конечную станцию направления Аэроэкспресс\";\"global_id\";";
    public static char[] s_separators = { ';', '\"' };

    public static bool ProcessFile(string path, out string msg)
    {
        Header(path);
        try
        {
            msg = "ok";
            return true;
        }
        catch (Exception e)
        {
            msg = e.Message;
            return false;
        }
    }
    
    public static void Init()
    {
        try
        {
            DataFolder = Path.Join(Directory.GetCurrentDirectory(), "data");
            if (!Directory.Exists(DataFolder))
            {
                Directory.CreateDirectory(DataFolder);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Couldn't create data directory. Default was chosen");
            DataFolder = "";
        }

        Console.WriteLine($"Data directory selected: {DataFolder}");
    }
}