using System;
using System.Diagnostics.CodeAnalysis;

namespace LR.Standard;

public static class StandardDateTimeExtensions
{
    public static string FormatSortable(this DateTime date, string default_value = "")
    {
        if (IsNull(date))
        {
            return default_value;
        }
        else
        {
            return date.ToString("yyyy-MM-ddTHH:mm:ss.fff");
        }
    }

    public static string FormatSortable(this DateTime? date, string default_value = "")
    {
        if (IsNull(date))
        {
            return default_value;
        }
        else
        {
            return date.Value.ToString("yyyy-MM-ddTHH:mm:ss.fff");
        }
    }

    public static string FormatInternational(this DateTime date, bool with_milliseconds = false, string default_value = "")
    {
        if (IsNull(date))
        {
            return default_value;
        }
        else if (with_milliseconds)
        {
            return date.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
        else
        {
            return date.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    public static string FormatInternational(this DateTime? date, bool with_milliseconds = false, string default_value = "")
    {
        if (IsNull(date))
        {
            return default_value;
        }
        else if (with_milliseconds)
        {
            return date.Value.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
        else
        {
            return date.Value.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    public static DateTime? NullIfMinValue(this DateTime? date)
    {
        return IsNull(date) ? null : date;
    }

    private static readonly DateTime Epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static DateTime FromUnixTimeMilliseconds(this long milliseconds)
    {
        if (milliseconds == 0)
        {
            return DateTime.MinValue;
        }

        if (milliseconds > 4102444800000)
        {
            return DateTime.MinValue;
        }

        return Epoch.AddMilliseconds(milliseconds);
    }

    public static DateTime FromUnixTimeSeconds(this long seconds)
    {
        if (seconds == 0)
        {
            return DateTime.MinValue;
        }

        if (seconds > 4102444800)
        {
            return DateTime.MinValue;
        }

        return Epoch.AddSeconds(seconds);
    }

    public static long GetCurrentUnixTimeMilliseconds()
    {
        return (long)( DateTime.UtcNow - Epoch ).TotalMilliseconds;
    }

    public static long GetCurrentUnixTimeSeconds()
    {
        return ToUnixTimeSeconds(DateTime.UtcNow);
    }

    public static long ToUnixTimeMilliseconds(this DateTime? dateTime)
    {
        if (IsNull(dateTime))
        {
            return 0;
        }

        if (dateTime.Value.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("dateTime is expected to be expressed as a UTC DateTime", "dateTime"); // .ToUniversalTime ()
        }

        return (long)( dateTime.Value.ToUniversalTime() - Epoch ).TotalMilliseconds;
    }

    public static long ToUnixTimeMilliseconds(this DateTime dateTime)
    {
        if (dateTime == DateTime.MinValue)
        {
            return 0;
        }

        if (dateTime.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("dateTime is expected to be expressed as a UTC DateTime", "dateTime"); // .ToUniversalTime ()
        }

        return (long)( dateTime.ToUniversalTime() - Epoch ).TotalMilliseconds;
    }

    public static long ToUnixTimeSeconds(this DateTime? dateTime)
    {
        if (IsNull(dateTime))
        {
            return 0;
        }

        if (dateTime.Value.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("dateTime is expected to be expressed as a UTC DateTime", "dateTime");
        }

        return (long)( dateTime.Value.ToUniversalTime() - Epoch ).TotalSeconds;
    }

    public static long ToUnixTimeSeconds(this DateTime dateTime)
    {
        if (dateTime == DateTime.MinValue)
        {
            return 0;
        }

        if (dateTime.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("dateTime is expected to be expressed as a UTC DateTime", "dateTime");
        }

        return (long)( dateTime.ToUniversalTime() - Epoch ).TotalSeconds;
    }

    public static bool IsNull([NotNullWhen(false)] this DateTime? dateTime)
    {
        if (dateTime.HasValue)
        {
            return dateTime.Value == DateTime.MinValue;
        }
        else
        {
            return true;
        }
    }

    public static bool IsNull(this DateTime dateTime)
    {
        return dateTime == DateTime.MinValue;
    }

    public static bool IsNotNull([NotNullWhen(true)] this DateTime? dateTime)
    {
        return !IsNull(dateTime);
    }

    public static bool IsNotNull(this DateTime dateTime)
    {
        return !IsNull(dateTime);
    }
}
