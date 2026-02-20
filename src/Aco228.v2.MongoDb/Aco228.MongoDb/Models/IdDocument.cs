using Aco228.MongoDb.Models.Attributes;
using MongoDB.Bson;

namespace Aco228.MongoDb.Models;

public class IdDocument
{
    [MongoIndex] public string SlugId { get; set; }
    [MongoIndex] public ObjectId Id { get; set; }
    [MongoIndex] public string Name { get; set; } = "";
    public string? Description { get; set; } = "";

    public static IdDocument CreateFrom<T>(T document) where T : SlugDocument
        => new()
        {
            SlugId = document.SlugId,
            Id = document.Id,
            Name = document.Name,
            Description = document.Description,
        };
}