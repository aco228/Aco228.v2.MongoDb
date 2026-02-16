using System.Text.Json.Serialization;
using Aco228.MongoDb.Models.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aco228.MongoDb.Models;

[Serializable]
[BsonIgnoreExtraElements]
public abstract class MongoDocument : MongoDocumentInternal
{
    [MongoIndex] [JsonIgnore]
    public long CreatedUtc { get; set; }
    
    [MongoIndex] [JsonIgnore] 
    public long UpdatedUtc { get; set; }

    internal virtual bool CanBeDeleted { get; } = true;
}

[Serializable]
[BsonIgnoreExtraElements]
public abstract class MongoLite : MongoDocument
{
    
}