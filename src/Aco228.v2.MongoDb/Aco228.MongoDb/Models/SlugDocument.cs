using Aco228.MongoDb.Models.Attributes;

namespace Aco228.MongoDb.Models;

public abstract class SlugDocument : MongoLite
{
    [MongoIndex] public string SlugId { get; set; }
    [MongoIndex] public string Name { get; set; }
    [MongoIndex] public string? Description { get; set; }
}