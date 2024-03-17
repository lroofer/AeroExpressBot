using System.Collections;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace FileProcessing;

/// <summary>
/// The class is a collection of trips. It has methods to operate with them.
/// </summary>
public class Trips : IEnumerable<TripInfo>
{
    /// <summary>
    /// Property that stores a collection.
    /// </summary>
    private TripInfo[] All { get; }

    /// <returns>True if collection is empty.</returns>
    public bool Empty() => All.Length == 0;

    /// <summary>
    /// The constructor that creates a new object from the array of trips.
    /// </summary>
    public Trips(TripInfo[] allTrips)
    {
        All = allTrips;
    }

    public int Count => All.Length;

    public TripInfo this[int index]
    {
        get => All[index];
        set => All[index] = value;
    }

    /// <summary>
    /// The constructor that creates an empty instance.
    /// </summary>
    public Trips()
    {
        All = Array.Empty<TripInfo>();
    }

    /// <summary>
    /// The constructor creates an object based on the data that was collected from the csv file.
    /// </summary>
    /// <param name="list">The data from csv file in the specified format.</param>
    public Trips(IReadOnlyList<string> list)
    {
        All = new TripInfo[list.Count];
        for (var i = 0; i < list.Count; ++i)
        {
            All[i] = new TripInfo(list[i].Split(Manager.s_separators, StringSplitOptions.RemoveEmptyEntries));
        }
    }

    public void Sort(Func<TripInfo, TripInfo, int> comparer)
    {
        Array.Sort(All, (info, tripInfo) => comparer(info, tripInfo));
    }

    public string ExportJson()
    {
        var options1 = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            WriteIndented = true
        };
        return JsonSerializer.Serialize(All, options1);
    }

    /// <summary>
    /// The method creates a deep copy of the object.
    /// </summary>
    /// <param name="trips">Reference to the cloned object.</param>
    public void Clone(out Trips trips)
    {
        trips = new Trips(All);
    }

    /// <summary>
    /// The method exports the info about trips to the format that can be written to a csv file.
    /// </summary>
    public string[] Export()
    {
        var export = new string[All.Length + 2];
        export[0] = Manager.FormatNames;
        export[1] = Manager.FormatColumns;
        for (var i = 0; i < All.Length; ++i)
        {
            export[i + 2] = All[i].ToString();
        }

        return export;
    }

    public IEnumerator<TripInfo> GetEnumerator()
        => ((IEnumerable<TripInfo>)All).GetEnumerator();


    /// <summary>
    /// Overriden method converts the trips information into a string that matches the specified format and can be added to a csv file.
    /// </summary>
    /// <returns>String in the format: "ID1";...;global_id1;\n"ID2"...</returns>
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.AppendLine($"There are {Count} trips in your file");
        int count = 0;
        foreach (var trip in All)
        {
            if (count++ >= 5) break;
            sb.Append(trip + "\n");
        }

        return sb.ToString();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Overloaded operator +, it appends new trips to the current one.
    /// </summary>
    public static Trips operator +(Trips a, Trips b)
    {
        int n = a.All.Length + b.All.Length;
        var allTrips = new TripInfo[n];
        for (int i = 0; i < n; ++i)
        {
            if (i < a.All.Length)
            {
                allTrips[i] = a.All[i];
            }
            else
            {
                allTrips[i] = b.All[i - a.All.Length];
            }
        }

        return new Trips(allTrips);
    }
}