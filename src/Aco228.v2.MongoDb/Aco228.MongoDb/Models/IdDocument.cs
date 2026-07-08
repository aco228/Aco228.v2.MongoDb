using Aco228.MongoDb.Models.Attributes;
using MessagePack;
using MongoDB.Bson;

namespace Aco228.MongoDb.Models;

[Serializable]
[MessagePackObject]
public class IdDocument
{
    [Key(0)] [MongoIndex] public string SlugId { get; set; }
    [Key(1)] [MongoIndex] public ObjectId Id { get; set; }
    [Key(2)] [MongoIndex] public string Name { get; set; } = "";
    [Key(3)] public string? Description { get; set; } = "";

    public static IdDocument? CreateFrom<T>(T? document) where T : SlugDocument
    {
        if(document == null) return null;
        return new()
        {
            Description = document.Description,
            Id = document.Id,
            Name = document.Name,
            SlugId = document.SlugId,
        };
        
    }

    public IdDocument Copy()
        => new()
        {
            Description = Description,
            Id = Id,
            Name = Name,
            SlugId = SlugId,
        };
}