using Aco228.MongoDb.Models.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aco228.MongoDb.Models;

[Serializable]
[BsonIgnoreExtraElements]
public class MongoDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public ObjectId Id { get; set; }
    
    [MongoIndex] public long CreatedUtc { get; set; }
    [MongoIndex] public long UpdatedUtc { get; set; }
}