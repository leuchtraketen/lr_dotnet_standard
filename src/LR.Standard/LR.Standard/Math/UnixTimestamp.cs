using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json.Serialization;

namespace LR.Standard;

[JsonConverter(typeof(UnixTimestampWrittenOutConverter))]
[StructLayout(LayoutKind.Sequential)]
public readonly struct UnixTimestamp : IComparable, IComparable<UnixTimestamp>, IEquatable<UnixTimestamp>
{
    private readonly long _timestamp;

    public static readonly UnixTimestamp Epoch = new();
    public static readonly DateTime EpochDateTime = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static UnixTimestamp Now => FromDateTime(DateTime.UtcNow);

    // public static string GetFormatLocal (Wrapper wrapper) => "#yyyy-MM-dd HH:mm".Translate (wrapper);

    [JsonIgnore] public long UnixTimeSeconds => this._timestamp;
    public DateOnly Date => DateOnly.FromDateTime(this.DateTime);
    public DateOnly LocalDate => DateOnly.FromDateTime(this.DateTime.ToLocalTime());
    public TimeOfDay TimeOfDay => TimeOfDay.FromTimeSpan(this.DateTime.TimeOfDay);
    public TimeOfDay LocalTimeOfDay => TimeOfDay.FromTimeSpan(this.DateTime.ToLocalTime().TimeOfDay);

    private UnixTimestamp(long timestamp)
    {
        _timestamp = timestamp;
    }

    public DateTime DateTime
    {
        get
        {
            if (this._timestamp == 0)
            {
                return DateTime.MinValue;
            }

            if (this._timestamp > 4102444800)
            {
                return DateTime.MinValue;
            }

            try
            {
                return EpochDateTime.AddSeconds(this._timestamp);
            }
            catch (ArgumentOutOfRangeException)
            {
                return EpochDateTime;
            }
        }
    }

    public static UnixTimestamp FromUnixTimeSeconds(long seconds)
    {
        return new UnixTimestamp(seconds);
    }

    public static UnixTimestamp FromDateTimeOffset(DateTimeOffset? value)
    {
        if (value.HasValue && value.Value != default)
        {
            return FromUnixTimeSeconds(value.Value.ToUnixTimeSeconds());
        }

        return Epoch;
    }

    public static UnixTimestamp FromDateTimeOffset(DateTimeOffset value)
    {
        if (value != default)
        {
            return FromUnixTimeSeconds(value.ToUnixTimeSeconds());
        }

        return Epoch;
    }

    public static UnixTimestamp FromDateTime(DateTime? value)
    {
        return FromDateTime(value: value ?? DateTime.MinValue);
    }

    public static UnixTimestamp FromDateTime(DateTime value)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("dateTime is expected to be expressed as a UTC DateTime", "dateTime");
        }

        long seconds;
        if (value == DateTime.MinValue)
        {
            seconds = 0;
        }
        else
        {
            seconds = (long)( value.ToUniversalTime() - EpochDateTime ).TotalSeconds;
        }

        return new UnixTimestamp(seconds);
    }

    public static UnixTimestamp operator +(UnixTimestamp a, TimeSpan b)
    {
        return new UnixTimestamp(a._timestamp + (long)b.TotalSeconds);
    }

    public static UnixTimestamp operator -(UnixTimestamp a, TimeSpan b)
    {
        return new UnixTimestamp(a._timestamp - (long)b.TotalSeconds);
    }

    public static UnixTimestamp operator +(UnixTimestamp a, long b)
    {
        return new UnixTimestamp(a._timestamp + (long)b);
    }

    public static UnixTimestamp operator -(UnixTimestamp a, long b)
    {
        return new UnixTimestamp(a._timestamp - (long)b);
    }

    public static TimeSpan operator -(UnixTimestamp a, UnixTimestamp b)
    {
        return TimeSpan.FromSeconds(( a.IsNotNull() && b.IsNotNull() ) ? ( a._timestamp - b._timestamp ) : 0);
    }

    public static bool operator ==(UnixTimestamp t1, UnixTimestamp t2)
    {
        return t1._timestamp == t2._timestamp;
    }

    public static bool operator !=(UnixTimestamp t1, UnixTimestamp t2)
    {
        return t1._timestamp != t2._timestamp;
    }

    public static bool operator <(UnixTimestamp t1, UnixTimestamp t2)
    {
        return t1._timestamp < t2._timestamp;
    }

    public static bool operator <=(UnixTimestamp t1, UnixTimestamp t2)
    {
        return t1._timestamp <= t2._timestamp;
    }

    public static bool operator >(UnixTimestamp t1, UnixTimestamp t2)
    {
        return t1._timestamp > t2._timestamp;
    }

    public static bool operator >=(UnixTimestamp t1, UnixTimestamp t2)
    {
        return t1._timestamp >= t2._timestamp;
    }

    public override bool Equals(object? obj)
    {
        if (obj is UnixTimestamp)
        {
            return this._timestamp == ( (UnixTimestamp)obj )._timestamp;
        }

        return false;
    }

    public bool Equals(UnixTimestamp other)
    {
        return this._timestamp == other._timestamp;
    }

    public static bool Equals(UnixTimestamp t1, UnixTimestamp t2)
    {
        return t1._timestamp == t2._timestamp;
    }

    public override int GetHashCode()
    {
        return (int)this._timestamp ^ (int)( this._timestamp >> 32 );
    }

    public static int Compare(UnixTimestamp t1, UnixTimestamp t2)
    {
        if (t1._timestamp > t2._timestamp) return 1;
        if (t1._timestamp < t2._timestamp) return -1;
        return 0;
    }

    // Returns a value less than zero if this  object
    public int CompareTo(object? value)
    {
        if (value == null) return 1;
        if (!( value is UnixTimestamp ))
            return 0;
        long t = ( (UnixTimestamp)value )._timestamp;
        if (this._timestamp > t) return 1;
        if (this._timestamp < t) return -1;
        return 0;
    }

    public int CompareTo(UnixTimestamp value)
    {
        long t = value._timestamp;
        if (this._timestamp > t) return 1;
        if (this._timestamp < t) return -1;
        return 0;
    }

    public bool IsNull()
    {
        return this._timestamp <= 0;
    }

    public bool IsNotNull()
    {
        return this._timestamp > 0;
    }

    public UnixTimestamp IfNull(UnixTimestamp other)
    {
        return this.IsNotNull() ? this : other;
    }


    public string FormatSortable(string default_value = "")
    {
        if (this.IsNull())
        {
            return default_value;
        }
        else
        {
            return this.DateTime.ToString("yyyy-MM-ddTHH:mm:ss.fff");
        }
    }

    public string FormatInternational(bool with_milliseconds = false, string default_value = "")
    {
        if (this.IsNull())
        {
            return default_value;
        }
        else if (with_milliseconds)
        {
            return this.DateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
        else
        {
            return this.DateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }


    /*
public string FormatLocal (Wrapper wrapper, string default_value = "", string prefix = "", bool? colloquial = null)
{
    if (IsNull ())
    {
        return default_value;
    }
    else
    {
        string result = prefix + DateTime.ToLocalTime ().ToString (GetFormatLocal (wrapper));
        string timeFormatSuffix = "#time format suffix".Translate (wrapper);
        if (timeFormatSuffix.Length > 1)
        {
            result += " " + timeFormatSuffix;
        }

        if (!colloquial.HasValue) colloquial = ColloquialTimestampExtensions.IsEnabledByDefault ();
        if (colloquial.Value)
        {
            result = ColloquialTimestampExtensions.ToColloquialFormat (result);
        }

        return result;
    }
}
*/

    public override string ToString()
    {
        return this.UnixTimeSeconds.ToString();
    }
}
