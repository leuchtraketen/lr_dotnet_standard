using System;
using System.Text.Json.Serialization;

namespace LR.Standard;

[JsonConverter (typeof (TimeOfDayConverter))]
public struct TimeOfDay : IComparable, IComparable<TimeOfDay>, IEquatable<TimeOfDay>
{
    int _hour;
    int _min;
    int _sec;

    public int Hour => this._hour;
    public int Minute => this._min;
    public int Second => this._sec;

    public static readonly TimeOfDay Default = new ();

    public TimeOfDay (int hour, int min, int sec)
    {
        this._hour = hour;
        this._min = min;
        this._sec = sec;
    }

    private TimeOfDay (TimeSpan timespan)
    {
        this._hour = timespan.Hours + timespan.Days * 24;
        this._min = timespan.Minutes;
        this._sec = timespan.Seconds;
    }

    [JsonIgnore]
    public TimeSpan TimeSpan => new (this._hour, this._min, this._sec);
    [JsonIgnore]
    public long TotalSeconds => this._hour * 3600 + this._min * 60 + this._sec;
    [JsonIgnore]
    public UnixTimestamp UnixTimestamp => UnixTimestamp.FromUnixTimeSeconds (this.TotalSeconds);

    public static TimeOfDay Parse (string str)
    {
        string [] a = (str ?? string.Empty).Split (':');
        return a.Length switch
        {
            1 => new TimeOfDay (a [0].ToInteger (), 0, 0),
            2 => new TimeOfDay (a [0].ToInteger (), a [1].ToInteger (), 0),
            3 => new TimeOfDay (a [0].ToInteger (), a [1].ToInteger (), a [2].ToInteger ()),
            _ => TimeOfDay.Default,
        };
    }

    public static TimeOfDay FromTimeSpan (TimeSpan timespan)
    {
        return new TimeOfDay (timespan);
    }

    public static TimeOfDay FromUnixTimestamp (UnixTimestamp timestamp)
    {
        if (timestamp.IsNull ())
        {
            return Default;
        }
        TimeSpan timespan = timestamp.DateTime.ToLocalTime ().TimeOfDay;
        return new TimeOfDay (timespan);
    }

    public static TimeOfDay operator + (TimeOfDay a, TimeSpan b)
    {
        TimeSpan timespan = a.TimeSpan + b;
        return new TimeOfDay (timespan);
    }

    public static TimeOfDay operator - (TimeOfDay a, TimeSpan b)
    {
        TimeSpan timespan = a.TimeSpan - b;
        return new TimeOfDay (timespan);
    }

    public static TimeSpan operator + (TimeSpan a, TimeOfDay b)
    {
        TimeSpan timespan = a + b.TimeSpan;
        return timespan;
    }

    public static TimeSpan operator - (TimeSpan a, TimeOfDay b)
    {
        TimeSpan timespan = a - b.TimeSpan;
        return timespan;
    }

    public static TimeSpan operator - (TimeOfDay a, TimeOfDay b)
    {
        TimeSpan timespan = a.TimeSpan - b.TimeSpan;
        return timespan;
    }

    public static bool operator == (TimeOfDay t1, TimeOfDay t2)
    {
        return t1._hour == t2._hour && t1._min == t2._min && t1._sec == t2._sec;
    }

    public static bool operator != (TimeOfDay t1, TimeOfDay t2)
    {
        return t1._hour != t2._hour || t1._min != t2._min || t1._sec != t2._sec;
    }

    public static bool operator < (TimeOfDay t1, TimeOfDay t2)
    {
        return t1.TotalSeconds < t2.TotalSeconds;
    }

    public static bool operator <= (TimeOfDay t1, TimeOfDay t2)
    {
        return t1.TotalSeconds <= t2.TotalSeconds;
    }

    public static bool operator > (TimeOfDay t1, TimeOfDay t2)
    {
        return t1.TotalSeconds > t2.TotalSeconds;
    }

    public static bool operator >= (TimeOfDay t1, TimeOfDay t2)
    {
        return t1.TotalSeconds >= t2.TotalSeconds;
    }

    public override bool Equals (object? obj)
    {
        if (obj is TimeOfDay)
        {
            TimeOfDay other = ((TimeOfDay) obj);
            return this._hour == other._hour && this._min == other._min && this._sec == other._sec;
        }
        return false;
    }

    public bool Equals (TimeOfDay other)
    {
        return this._hour == other._hour && this._min == other._min && this._sec == other._sec;
    }

    public static bool Equals (TimeOfDay t1, TimeOfDay t2)
    {
        return t1._hour == t2._hour && t1._min == t2._min && t1._sec == t2._sec;
    }

    public override int GetHashCode ()
    {
        return (int) this.TotalSeconds;
    }

    public static int Compare (TimeOfDay t1, TimeOfDay t2)
    {
        long c1 = t1.TotalSeconds;
        long c2 = t2.TotalSeconds;
        if (c1 > c2) return 1;
        if (c1 < c2) return -1;
        return 0;
    }

    // Returns a value less than zero if this  object
    public int CompareTo (object? value)
    {
        if (value == null) return 1;
        if (!(value is TimeOfDay))
            return 0;
        long t = ((TimeOfDay) value).TotalSeconds;
        long c = this.TotalSeconds;
        if (c > t) return 1;
        if (c < t) return -1;
        return 0;
    }

    public int CompareTo (TimeOfDay value)
    {
        long t = value.TotalSeconds;
        long c = this.TotalSeconds;
        if (c > t) return 1;
        if (c < t) return -1;
        return 0;
    }

    public bool IsNull ()
    {
        return this.TotalSeconds <= 0;
    }

    public bool IsNotNull ()
    {
        return !this.IsNull ();
    }

    public TimeOfDay IfNull (TimeOfDay other)
    {
        return this.IsNotNull () ? this : other;
    }

    public string Format (string format = "HH:mm:ss")
    {
        if (format is null || string.IsNullOrEmpty (format))
            format = "HH:mm:ss";

        return format
            .Replace ("HH", $"{this._hour:D2}")
            .Replace ("mm", $"{this._min:D2}")
            .Replace ("ss", $"{this._sec:D2}");
    }

    public override string ToString ()
    {
        return $"{this._hour:D2}:{this._min:D2}:{this._sec:D2}";
    }
}
