using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aco228.MongoDb.Models;


public class MongoDocumentInternal
{
    [BsonId] [JsonIgnore]
    [BsonRepresentation(BsonType.String)]
    public ObjectId Id { get; set; }
    
    private MongoTrackingObject? _trackingObject;

    public bool HasTracking() => _trackingObject?.HasTracking() == true;
    public MongoTrackingObject? GetTrackingObject() => _trackingObject;
    
    public MongoTrackingObject StartTracking()
    {
        _trackingObject = new MongoTrackingObject(this, GetType()).StartTracking();
        return _trackingObject;
    }
}