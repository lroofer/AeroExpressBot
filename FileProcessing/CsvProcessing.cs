using System.Text;

namespace FileProcessing;

public class CsvProcessing
{
    /// <summary>
    /// A method reads data from the given stream. And checks it's compatability.
    /// </summary>
    /// <param name="stream">Open stream</param>
    /// <returns>The array of processed trips</returns>
    /// <exception cref="ArgumentException">Data couldn't be processed</exception>
    public Trips Read(Stream stream)
    {
        string[] lines;
        try
        {
            lines = ReadLines(stream, Encoding.Default).ToArray();
        }
        catch (Exception e)
        {
            throw new ArgumentException($"Data file couldn't be processed: {e.Message}");
        }

        if (!CheckFileFormat(in lines))
            throw new ArgumentException("CSV file doesn't meet the format");
        return new Trips(lines);
    }

    /// <summary>
    /// Prepares data for writing.
    /// </summary>
    /// <param name="trips">Data to write.</param>
    /// <param name="path">Path to temp file.</param>
    /// <returns>A readable stream.</returns>
    public async Task<Stream> Write(Trips trips, string path)
    {
        await UpdateCurrent(path, trips);
        return File.OpenRead(path);
    }

    /// <summary>
    /// The method checks the structure of the incoming data.
    /// </summary>
    /// <returns>true if correct.</returns>
    private bool CheckFileFormat(in string[] lines)
    {
        for (var i = 2; i < lines.Length; ++i)
        {
            if (lines[i].Equals(string.Empty)) continue;
            var items = lines[i].Split(';', StringSplitOptions.RemoveEmptyEntries);
            if (items.Length != 7) return false;
            break;
        }

        return true;
    }

    private async Task UpdateCurrent(string path, Trips trips)
        => await File.WriteAllLinesAsync(path, trips.Export());

    private IEnumerable<string> ReadLines(Stream stream, Encoding encoding)
    {
        var cntLines = 0;
        using var reader = new StreamReader(stream, encoding: encoding);
        while (reader.ReadLine() is { } line)
        {
            if (cntLines++ < 2) continue;
            yield return line;
        }
    }
}