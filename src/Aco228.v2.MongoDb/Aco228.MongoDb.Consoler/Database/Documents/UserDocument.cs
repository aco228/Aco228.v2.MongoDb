using Aco228.MongoDb.Models;
using Aco228.MongoDb.Models.Attributes;
using MongoDB.Bson.Serialization.Attributes;

namespace Aco228.MongoDb.Consoler.Database.Documents;

[BsonCollection("User")]
[BsonIgnoreExtraElements]
public class UserDocument : MongoDocument
{
    [MongoIndex]
    public string Username { get; set; }
    
    public int SomeIndex { get; set; }
    public string SomeData { get; set; }
    public string SomeExtraData { get; set; }
    
    [MongoIndex]
    public string SetBck { get; set; }
}

public class UserProjection : MongoProjection<UserDocument>
{
    [ProjectMap(nameof(UserDocument.SomeIndex))]
    public int DasIstIndex { get; set; }
}