using MongoDB.Bson;

namespace Aco228.MongoDb.Models;

public class ETagDocument
{
    public ObjectId Id { get; set; }
    public long UpdatedUtc { get; set; }
}