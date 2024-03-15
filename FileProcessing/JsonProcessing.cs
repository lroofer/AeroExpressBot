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

    public Trips Read(Stream stream)
    {
        throw new NotImplementedException();
    }
}