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

    public List<string> Data { get; set; } = new();
    public HashSet<string> Hash { get; set; } = new();
    public Dictionary<string, int> Dicts { get; set; } = new();
    public UserDocExtra? Extra { get; set; }
}

[BsonIgnoreExtraElements]
public class UserDocExtra
{
    public string Name { get; set; }
}

public class UserProjection : MongoProjection<UserDocument>
{
    [ProjectMap(nameof(UserDocument.SomeIndex))]
    public int DasIstIndex { get; set; }
}