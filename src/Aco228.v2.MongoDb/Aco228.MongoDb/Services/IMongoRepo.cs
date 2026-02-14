using System.Linq.Expressions;
using Aco228.MongoDb.Models;
using Aco228.MongoDb.Models.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Aco228.MongoDb.Services;

public interface IMongoRepo<TDocument>
    where TDocument : MongoDocument
{
    internal void Configure(BsonCollectionAttribute configuration, IMongoDbContext context);
    internal void GuardConfiguration();
    
    IMongoCollection<TDocument> GetCollection();
    IQueryable<TDocument> AsQueryable() => GetCollection().AsQueryable();
    IMongoTransaction<TDocument> GetTransactionalManager() => new MongoTransaction<TDocument>(this);
    
    public List<TDocument> LoadAll() => AsQueryable().ToList();
    public Task<List<TDocument>> LoadAllAsync() => AsQueryable().ToListAsync();
    public LoadSpecification<TDocument, TDocument> NoTrack() => new(this, false);
    public LoadSpecification<TDocument, TDocument> Track() => new(this, true);
    public LoadSpecification<TDocument, TProjection> Project<TProjection>() where TProjection : class => new(this, false);
    public Task<TDocument?> FirstOrDefault(Expression<Func<TDocument, bool>>? filter) => AsQueryable().FirstOrDefaultAsync(filter);

    public LoadSpecification<TDocument, TProjection> TrackProject<TProjection>() where TProjection : MongoProjection<TDocument> => new(this, true);
}