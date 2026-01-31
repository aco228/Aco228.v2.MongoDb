using Aco228.MongoDb.Models;
using Aco228.MongoDb.Models.Attributes;
using Aco228.MongoDb.Strategies;
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
    public RepoLoadStrategy<TDocument, TDocument> LoadStrategy() => new (this);
    public RepoLoadSpecification<TDocument> Load() => new(this);
    public RepoLoadStrategy<TDocument, TProject> Project<TProject>() where TProject : class => new (this);
    
}