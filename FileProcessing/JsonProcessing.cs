using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace FileProcessing;

public class JsonProcessing
{
    public async Task<Stream> Write(Trips trips, string path)
    {
        await UpdateCurrent(path, trips);
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

    /// <summary>
    /// Updates the current temp file.
    /// </summary>
    /// <param name="path">Path to temp file.</param>
    /// <param name="trips">Data to update from.</param>
    private async Task UpdateCurrent(string path, Trips trips)
        => await File.WriteAllTextAsync(path, trips.ExportJson());
}