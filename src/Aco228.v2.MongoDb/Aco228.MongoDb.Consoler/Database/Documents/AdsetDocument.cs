using Aco228.MongoDb.Models;
using Aco228.MongoDb.Models.Attributes;
using MongoDB.Bson.Serialization.Attributes;

namespace Aco228.MongoDb.Consoler.Database.Documents;

[BsonIgnoreExtraElements]
[Serializable]
public abstract class MongoAccountDocument : MongoDocument
{
    public required Guid AccountId { get; set; }
}

[BsonCollection("facebookAdsets", typeof(IArbDbContext))]
public class AdsetDocument : MongoDocument
{   
    public string? CopiedAdsetName { get; set; }
    public long? CopiedAdsetId { get; set; }
    public int? DailyBudget { get; set; }
    public int? DailyBudgetBeforePause { get; set; } = null;
    public string? OfferName { get; set; }
    public DateTime? UtcDatePaused { get; set; }
    public DateTime? ScheduledStart { get; set; }
    public bool IsGeoExpand { get; set; } = false;
    
    public short? NumberOfActivationErrors { get; set; } = null;

    /// <summary>
    /// How much time adset was active but it was not detected in stats
    /// </summary>
    public int? MissingErrorCount { get; set; } = null;
}