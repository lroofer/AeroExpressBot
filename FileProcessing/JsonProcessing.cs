using System.Text;
using System.Text.Json;

namespace FileProcessing;

public class JsonProcessing
{
    private void UpdateCurrent(string path, Trips trips)
    {
        var lines = trips.ExportJson();
        File.WriteAllText(path, lines, Encoding.Unicode); // TODO: Make async.
    }
    public Stream Write(Trips trips, string path)
    {
        UpdateCurrent(path, trips);
        return File.OpenRead(path);
    }

    public async Task<Trips> Read(Stream stream)
    {
        return new Trips(await JsonSerializer.DeserializeAsync<TripInfo[]>(stream) ?? Array.Empty<TripInfo>());
    }
}