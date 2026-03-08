using Aco228.MongoDb.Models.Attributes;
using MongoDB.Bson;

namespace Aco228.MongoDb.Models;

public class IdDocument
{
    [MongoIndex] public string SlugId { get; set; }
    [MongoIndex] public ObjectId Id { get; set; }
    [MongoIndex] public string Name { get; set; } = "";
    public string? Description { get; set; } = "";

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