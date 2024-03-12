using System.Text;

namespace FileProcessing;

public class CsvProcessing
{
    public Stream Write(Trips trips)
    {
        throw new NotImplementedException();
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