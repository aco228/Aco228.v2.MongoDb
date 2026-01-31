using Aco228.MongoDb.Models;
using Aco228.MongoDb.Models.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Aco228.MongoDb.Services;

public interface IMongoRepo<TDocument>
    where TDocument : MongoDocument
{
    internal void Configure(BsonCollectionAttribute configuration, IMongoDbContext context);
    IMongoCollection<TDocument> GetCollection();
    IQueryable<TDocument> AsQueryable() => GetCollection().AsQueryable();
    IMongoRepoTransactionalManager<TDocument> GetTransactionalManager() => new MongoRepoTransactionalManager<TDocument>(this);
    internal void GuardConfiguration();

    public List<TDocument> LoadAll() => AsQueryable().ToList();
    public Task<List<TDocument>> LoadAllAsync() => AsQueryable().ToListAsync();
    public LoadSpecification<TDocument, TDocument> Load() => new(this);
    public LoadSpecification<TDocument, TProjection> Project<TProjection>() where TProjection :  class => new(this);
    
}