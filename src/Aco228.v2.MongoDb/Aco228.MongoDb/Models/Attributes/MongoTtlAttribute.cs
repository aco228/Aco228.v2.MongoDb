namespace Aco228.MongoDb.Models.Attributes;

/// <summary>
/// Creates a TTL index on the property, Mongo deletes the document once the property value is older than the expiry.
/// Property must be DateTime (stored as BSON date), Mongo ignores TTL on numbers so unix CreatedUtc can't be used.
/// Replaces [MongoIndex] on the same property. Mongo removes expired documents about once a minute, not exactly on time.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class MongoTtlAttribute : Attribute
{
    public int Days { get; set; }
    public int Hours { get; set; }
    public int Minutes { get; set; }
    public int Seconds { get; set; }

    public long ExpireAfterSeconds => Days * 86_400L + Hours * 3_600L + Minutes * 60L + Seconds;
}
