using Aco228.MongoDb.Models;
using Aco228.MongoDb.Models.Attributes;
using MongoDB.Driver;

namespace Aco228.MongoDb.Services;

public class MongoRepo<TDocument> : IMongoRepo<TDocument>
    where TDocument : MongoDocument
{
    public string CollectionName { get; private set; }
    private bool _isConfigured = false;
    private IMongoCollection<TDocument>? _collection;
    private IMongoDatabase? _database;

    public void Configure(BsonCollectionAttribute configurationAttribute, IMongoDbContext context)
    {
        CollectionName = configurationAttribute.CollectionName;
        _database = context.GetDatabase();
        _collection = _database.GetCollection<TDocument>(CollectionName);
        _isConfigured = true;
    }

    public IMongoCollection<TDocument> GetCollection() => _collection!;

    public void GuardConfiguration()
    {
        if (!_isConfigured) throw new InvalidOperationException("The collection configuration is not configured");
        if (_collection == null) throw new InvalidOperationException("Collection is null");
        if (_database == null) throw new InvalidOperationException("Database is null");
    }

    public Task<bool> AnyAsync() => _collection.Find(FilterDefinition<TDocument>.Empty).Limit(1).AnyAsync();
    public bool Any() => _collection.Find(FilterDefinition<TDocument>.Empty).Limit(1).Any();

    public Task<long> EstimateCountAsync() => _collection?.EstimatedDocumentCountAsync();
    public long EstimateCount() => _collection?.EstimatedDocumentCount() ?? 0;
    public async Task DropTable(bool areYouSure)
    {
        if (areYouSure == false) return;
        await _database!.DropCollectionAsync(CollectionName);
    }
}