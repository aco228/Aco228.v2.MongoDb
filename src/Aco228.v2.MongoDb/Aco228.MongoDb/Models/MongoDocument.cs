using System.Text.Json.Serialization;
using Aco228.MongoDb.Models.Attributes;
using MessagePack;
using MongoDB.Bson.Serialization.Attributes;

namespace Aco228.MongoDb.Models;

[Serializable]
[BsonIgnoreExtraElements]
public abstract class MongoDocument : MongoDocumentInternal
{
    [MongoIndex]
    [JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public long CreatedUtc { get; set; } = DT.GetUnix();
    
    [MongoIndex] [JsonIgnore] [Newtonsoft.Json.JsonIgnore] 
    public long UpdatedUtc { get; set; } = DT.GetUnix();

    internal virtual bool CanBeDeleted { get; } = true;

    protected virtual void OnBeforeSave() { }
    
    internal void BeforeSave()
    {
        OnBeforeSave();
    }   
    

}

[Serializable]
[BsonIgnoreExtraElements]
public abstract class MongoLite : MongoDocument
{
    
}