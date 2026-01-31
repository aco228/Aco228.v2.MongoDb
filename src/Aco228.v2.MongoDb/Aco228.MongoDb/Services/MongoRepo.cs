using Aco228.MongoDb.Extensions;
using Aco228.MongoDb.Models;
using Aco228.MongoDb.Models.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aco228.MongoDb.Services;

public class MongoRepo<TDocument> : IMongoRepo<TDocument>
    where TDocument : MongoDocument
{
    private bool _isConfigured = false;
    private IMongoCollection<TDocument>? _collection;
    private IMongoDatabase? _database;

    public void Configure(BsonCollectionAttribute configurationAttribute, IMongoDbContext context)
    {
        _database = context.GetDatabase();
        _collection = _database.GetCollection<TDocument>(configurationAttribute.CollectionName);
        _isConfigured = true;
    }

    public IMongoCollection<TDocument> GetCollection() => _collection!;
    
    public void GuardConfiguration()
    {
        if (!_isConfigured) throw new InvalidOperationException("The collection configuration is not configured");
        if (_collection == null) throw new InvalidOperationException("Collection is null");
        if (_database == null) throw new InvalidOperationException("Database is null");
    }
    
    
    
}