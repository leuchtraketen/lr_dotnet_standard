using System;
using System.Diagnostics;

namespace LR.Standard;

public static class HighResolutionTimer
{
    private static readonly long _start_timestamp_us;
    private static readonly Stopwatch _start_stopwatch;

    static HighResolutionTimer()
    {
        TimeSpan epochTicks = new(new DateTime(1970, 1, 1).Ticks);
        TimeSpan unixTicks = new TimeSpan(DateTime.UtcNow.Ticks) - epochTicks;
        _start_timestamp_us = (long)( 1000_000 * unixTicks.TotalSeconds );
        _start_stopwatch = Stopwatch.StartNew();
    }

    public static long TimestampMicroseconds
    {
        get => _start_timestamp_us + _start_stopwatch.ElapsedTicks * 1000_000 / Stopwatch.Frequency;
    }
}
