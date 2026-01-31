using Aco228.MongoDb.Models;
using Aco228.MongoDb.Models.Attributes;
using MongoDB.Bson.Serialization.Attributes;

namespace Aco228.MongoDb.Consoler.Database.Documents;

[BsonCollection("User", typeof(ILocalDbContext))]
[BsonIgnoreExtraElements]
public class UserDocument : MongoDocument
{
    [MongoIndex]
    public string Username { get; set; }
    
    public int SomeIndex { get; set; }
    public string SomeData { get; set; }
}

public class UserProjection
{
    [ProjectMap(nameof(UserDocument.SomeIndex))]
    public int DasIstIndex { get; set; }
}