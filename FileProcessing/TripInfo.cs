using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

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
    public string Line { get; set;  }

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
    public string StationEnd { get; set;  }

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
    public string GlobalId { get; set; }

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

    public TripInfo()
    {
        Id = "1";
        StationStart = "None";
        Line = "None";
        TimeStart = "12:32";
        StationEnd = "None";
        TimeEnd = "14:21";
        GlobalId = "untitled";
    }
    
    public TripInfo(string id, string stationStart, string line, string timeStart, string stationEnd, string timeEnd, string globalId)
    {
        Id = id;
        StationStart = stationStart;
        Line = line;
        TimeStart = timeStart;
        StationEnd = stationEnd;
        TimeEnd = timeEnd;
        GlobalId = globalId;
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
