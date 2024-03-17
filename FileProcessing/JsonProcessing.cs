using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace FileProcessing;

public class JsonProcessing
{
    private void UpdateCurrent(string path, Trips trips)
    {
        var lines = trips.ExportJson();
        File.WriteAllText(path, lines); // TODO: Make async.
    }
    public Stream Write(Trips trips, string path)
    {
        UpdateCurrent(path, trips);
        return File.OpenRead(path);
    }

    public async Task<Trips> Read(Stream stream)
    {
        var options1 = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            WriteIndented = true
        };
        return new Trips(await JsonSerializer.DeserializeAsync<TripInfo[]>(stream, options1) ??
                         Array.Empty<TripInfo>());
    }
}