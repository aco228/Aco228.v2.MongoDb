using Aco228.MongoDb.Models.Attributes;
using MongoDB.Bson;

namespace Aco228.MongoDb.Models;

public class IdDocument
{
    [MongoIndex] public string SlugId { get; set; }
    [MongoIndex] public ObjectId Id { get; set; }
    [MongoIndex] public string Name { get; set; } = ""; 
}