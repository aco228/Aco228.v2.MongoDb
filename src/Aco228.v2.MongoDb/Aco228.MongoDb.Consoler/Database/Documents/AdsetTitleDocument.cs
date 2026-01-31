using Aco228.MongoDb.Models;
using Aco228.MongoDb.Models.Attributes;
using MongoDB.Bson.Serialization.Attributes;

namespace Aco228.MongoDb.Consoler.Database.Documents;

[BsonCollection("adsetTitleCollection", typeof(IArbDbContext))]
[BsonIgnoreExtraElements]
public class AdsetTitleDocument : MongoDocument
{
    [MongoIndex] public string BatchName { get; set; }
    [MongoIndex] public string Language { get; set; }
    public List<string> Texts { get; set; } = new();
}