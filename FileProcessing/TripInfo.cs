using System.Text;
using System.Text.Json.Serialization;

namespace FileProcessing;

/// <summary>
/// The class contains all the information about certain trip and all the required methods to operate with it.
/// </summary>
public class TripInfo
{
    private int _id;
    private string _timeStart;
    private string _timeEnd;
    
    [JsonPropertyName("Id")]
    public string Id
    {
        get => _id.ToString();
        set
        {
            if (!int.TryParse(value, out _id) || _id < 0)
            {
                throw new ArgumentException("Id must be positive integer");
            }
        }
    }
    
    [JsonPropertyName("StationStart")]
    public string StationStart{ get; set; }
    
    [JsonPropertyName("Line")]
    public string Line { get; }

    [JsonPropertyName("TimeStart")]
    public string TimeStart
    {
        get => _timeStart;
        set
        {
            var lst = value.Split(':');
            if (lst.Length != 2 || value.Length != 5)
                throw new ArgumentException("TimeStart field must meet the format HH:MM");
            if (!int.TryParse(lst[0], out var h) || !int.TryParse(lst[1], out var m))
            {
                throw new ArgumentException("TimeStart field must meet the format HH:MM");
            }

            if (h is < 0 or >= 24 || m is < 0 or >= 60)
            {
                throw new ArgumentException("Time is out of range");
            }

            _timeStart = value;
        }
    }
    [JsonPropertyName("StationEnd")]
    public string StationEnd { get; }

    [JsonPropertyName("TimeEnd")]
    public string TimeEnd
    {
        get => _timeEnd;
        set
        {
            var lst = value.Split(':');
            if (lst.Length != 2 || value.Length != 5)
                throw new ArgumentException("TimeStart field must meet the format HH:MM");
            if (!int.TryParse(lst[0], out var h) || !int.TryParse(lst[1], out var m))
            {
                throw new ArgumentException("TimeStart field must meet the format HH:MM");
            }

            if (h is < 0 or >= 24 || m is < 0 or >= 60)
            {
                throw new ArgumentException("Time is out of range");
            }

            _timeEnd = value;
        }
    }
    
    [JsonPropertyName("global_id")]
    private string GlobalId { get; }

    /// <summary>
    /// The constructor of the class TripInfo. It creates an object (new trip) from the array of properties.
    /// </summary>
    /// <param name="itemFormat">7 properties (corresponding to the given format).</param>
    /// <exception cref="ArgumentException">It'll be thrown in case 'itemFormat' doesn't contain exactly 7 properties.</exception>
    public TripInfo(string[] itemFormat)
    {
        if (itemFormat.Length != 7)
        {
            throw new ArgumentException("Data doesn't meet the format");
        }

        Id = itemFormat[0];
        StationStart = itemFormat[1];
        Line = itemFormat[2];
        TimeStart = itemFormat[3];
        StationEnd = itemFormat[4];
        TimeEnd = itemFormat[5];
        GlobalId = itemFormat[6];
    }

    /// <summary>
    /// Overriden method converts the trip information into a string that matches the specified format
    /// and can be added to a csv file.
    /// </summary>
    /// <returns>String in the format: "Id";...;global_id;</returns>
    public override string ToString()
    {
        return
            $"\"{Id}\";\"{StationStart}\";\"{Line}\";\"{TimeStart}\";\"{StationEnd}\";\"{TimeEnd}\";\"{GlobalId}\";";
    }

    /// <summary>
    /// The method writes all the properties to the given array.
    /// </summary>
    /// <param name="arr">Reference to the array</param>
    public void ToArray(out string[] arr)
    {
        arr = new []{ Id, StationStart, Line, TimeStart, StationEnd, TimeEnd, GlobalId };
    }
}

/// <summary>
/// The class is a collection of trips. It has methods to operate with them.
/// </summary>
public class Trips
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
        export[0] = Manager.FORMAT_NAMES;
        export[1] = Manager.FORMAT_COLUMNS;
        for (var i = 0; i < All.Length; ++i)
        {
            export[i + 2] = All[i].ToString();
        }

        return export;
    }
   /// <summary>
   /// Overrided method converts the trips information into a string that matches the specified format and can be added to a csv file.
   /// </summary>
   /// <returns>String in the format: "ID1";...;global_id1;\n"ID2"...</returns>
    public override string ToString()
    {
        StringBuilder sb = new();
        foreach (var trip in All)
        {
            sb.Append(trip + "\n");
        }

        return sb.ToString();
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