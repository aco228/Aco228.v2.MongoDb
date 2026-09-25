using Aco228.Common.Extensions;

namespace Aco228.MongoDb.Models;

public static class DT
{
    /// <summary>Custom epoch: 2026-01-01 00:00:00 UTC. Partial timestamps are offsets from this.</summary>
    public static readonly DateTime Epoch = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static DateTime GetDateTimeUtcToday()
        => DateTime.UtcNow.Date;

    public static long GetUtcToday()
        => GetDateTimeUtcToday().ToUnixTimestampSeconds();

    public static long GetUnix()
        => DateTime.UtcNow.ToUnixTimestampMilliseconds();

    public static long ToDT(this DateTime dt)
        => dt.ToUnixTimestampMilliseconds();

    public static long ToDtAddMinutes(int minutes)
        => DateTime.UtcNow.AddMinutes(minutes).ToUnixTimestampMilliseconds();

    public static long ToDtAddSeconds(int seconds)
        => DateTime.UtcNow.AddSeconds(seconds).ToUnixTimestampMilliseconds();

    // ---------- Partial (uint, offset from 2026 epoch) ----------

    /// <summary>Seconds since 2026-01-01 UTC. Valid until ~Feb 2162.</summary>
    public static uint GetPartialUnixSec()
        => DateTime.UtcNow.ToPartialSec();

    /// <summary>
    /// Milliseconds since 2026-01-01 UTC.
    /// WARNING: uint ms only covers ~49.7 days (until ~2026-02-19). Throws OverflowException after that.
    /// </summary>
    public static uint GetPartialUnixMs()
        => DateTime.UtcNow.ToPartialMs();

    public static uint ToPartialSec(this DateTime dt)
        => checked((uint)((AsUtc(dt).Ticks - Epoch.Ticks) / TimeSpan.TicksPerSecond));

    public static uint ToPartialMs(this DateTime dt)
        => checked((uint)((AsUtc(dt).Ticks - Epoch.Ticks) / TimeSpan.TicksPerMillisecond));

    public static DateTime FromPartialSec(uint seconds)
        => Epoch.AddSeconds(seconds);

    public static DateTime FromPartialMs(uint ms)
        => Epoch.AddMilliseconds(ms);

    /// <summary>Converts a partial value to a full unix timestamp (ms) if you need a long again.</summary>
    public static long PartialSecToUnixMs(uint seconds)
        => FromPartialSec(seconds).ToUnixTimestampMilliseconds();

    // Unspecified kind is treated as UTC (not local) to avoid silent timezone shifts.
    private static DateTime AsUtc(DateTime dt) => dt.Kind switch
    {
        DateTimeKind.Utc => dt,
        DateTimeKind.Local => dt.ToUniversalTime(),
        _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc),
    };
}