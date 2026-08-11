using Aco228.Common.Extensions;

namespace Aco228.MongoDb.Models;

public static class DT
{
    public static long GetUnix()
        => DateTime.UtcNow.ToUnixTimestampMilliseconds();
    
    public static long ToDT(this DateTime dt) 
        => dt.ToUnixTimestampMilliseconds();

    public static long ToDtAddMinutes(int minutes)
        => DateTime.UtcNow.AddMinutes(minutes).ToUnixTimestampMilliseconds();

}