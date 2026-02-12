using System.Linq.Expressions;
using Aco228.MongoDb.Models;
using Aco228.MongoDb.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aco228.MongoDb.Extensions.RepoExtensions;

public static class MongoRepoLoadSingle
{
    public static long Count<TDocument>(
        this IMongoRepo<TDocument> repo, 
        Expression<Func<TDocument, bool>> filter) 
        where TDocument : MongoDocument
    {
        return repo.NoTrack().FilterBy(filter).Count();
    }
    
    public static Task<long> CountAsync<TDocument>(
        this IMongoRepo<TDocument> repo, 
        Expression<Func<TDocument, bool>> filter) 
        where TDocument : MongoDocument
    {
        return repo.NoTrack().FilterBy(filter).CountAsync();
    }
    //
    // public static TDocument? FirstOrDefault<TDocument>(
    //     this IMongoRepo<TDocument> repo, 
    //     Expression<Func<TDocument, bool>> filter,
    //     int? limit = null) 
    //     where TDocument : MongoDocument
    // {
    //     return repo.NoTrack().FilterBy(filter).Limit(limit).FirstOrDefault();
    // }
    //
    // public static Task<TDocument?> FirstOrDefaultAsync<TDocument>(
    //     this IMongoRepo<TDocument> repo, 
    //     Expression<Func<TDocument, bool>> filter,
    //     int? limit = null) 
    //     where TDocument : MongoDocument
    // {
    //     return repo.NoTrack().FilterBy(filter).Limit(limit).FirstOrDefaultAsync();
    // }
    //
    // public static TDocument? FindById<TDocument>(
    //     this IMongoRepo<TDocument> repo, 
    //     ObjectId objectId) 
    //     where TDocument : MongoDocument
    // {
    //     return repo.NoTrack().FirstOrDefault(d => d.Id == objectId);
    // }
    //
    // public static TDocument? FindById<TDocument>(
    //     this IMongoRepo<TDocument> repo, 
    //     string id) 
    //     where TDocument : MongoDocument
    // {
    //     return repo.NoTrack().FirstOrDefault(d => d.Id == ObjectId.Parse(id));
    // }
    //
    // public static Task<TDocument?> FindByIdAsync<TDocument>(
    //     this IMongoRepo<TDocument> repo, 
    //     ObjectId objectId) 
    //     where TDocument : MongoDocument
    // {
    //     return repo.NoTrack().FirstOrDefaultAsync(d => d.Id == objectId);
    // }
    //
    //
    // public static Task<TDocument?> FindByIdAsync<TDocument>(
    //     this IMongoRepo<TDocument> repo, 
    //     string objectId) 
    //     where TDocument : MongoDocument
    // {
    //     return repo.NoTrack().FirstOrDefaultAsync(d => d.Id == ObjectId.Parse(objectId));
    // }
    
}