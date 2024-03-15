using System.Text;

namespace FileProcessing;

public class CsvProcessing
{
    /// <summary>
    /// The method checks the structure of the incoming data.
    /// </summary>
    /// <returns>true if correct.</returns>
    private bool CheckFileFormat(in string[] lines)
    {
        if (lines.Length < 2) return false;

        if (!lines[0].Equals(Manager.FormatNames))
            return false;

        if (!lines[1].Equals(Manager.FormatColumns))
            return false;
        for (int i = 2; i < lines.Length; ++i)
        {
            if (lines[i].Equals(string.Empty)) continue;
            var items = lines[i].Split(';', StringSplitOptions.RemoveEmptyEntries);
            if (items.Length != 7) return false;
            break;
        }
        return true;
    }

    private void UpdateCurrent(string path, Trips trips)
    {
        var lines = trips.Export();
        File.WriteAllLines(path, lines); // TODO: Make async.
    }
    public Stream Write(Trips trips, Manager manager, string username)
    {
        string path;
        UpdateCurrent(
            path = manager.GetUserFileName(username, "csv"), trips);
        return File.OpenRead(path);
    }

    private IEnumerable<string> ReadLines(Stream stream, Encoding encoding)
    {
        var cntLines = 0;
        using var reader = new StreamReader(stream, encoding: Encoding.UTF8);
        while (reader.ReadLine() is { } line)
        {
            if (cntLines++ < 2) continue;
            yield return line;
        }
    }
    public Trips Read(Stream stream)
    {
        var lines = ReadLines(stream, Encoding.UTF8);
        
        return new Trips(lines.ToList());
    }
}