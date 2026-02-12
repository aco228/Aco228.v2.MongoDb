using System.Linq.Expressions;
using Aco228.MongoDb.Models;
using Aco228.MongoDb.Services;

namespace Aco228.MongoDb.Extensions.RepoExtensions;

public static class MongoRepoLoadListExtensions
{
    // public static List<TDocument> FilterAndOrder<TDocument>(
    //     this IMongoRepo<TDocument> repo, 
    //     OrderDirection orderDirection,
    //     string parameterName,
    //     Expression<Func<TDocument, bool>>? filter = null,
    //     int? limit = null) 
    //     where TDocument : MongoDocument
    // {
    //     return repo.NoTrack().FilterBy(filter).Limit(limit).OrderByPropertyName(orderDirection, parameterName).ToEnumerable().ToList();
    // }
    //
    // public static Task<List<TDocument>> FilterAndOrderAsync<TDocument>(
    //     this IMongoRepo<TDocument> repo, 
    //     OrderDirection orderDirection,
    //     string parameterName,
    //     Expression<Func<TDocument, bool>>? filter = null,
    //     int? limit = null) 
    //     where TDocument : MongoDocument
    // {
    //     return repo.NoTrack().FilterBy(filter).Limit(limit).OrderByPropertyName(orderDirection, parameterName).ToListAsync();
    // }
    //
    // public static List<TDocument> FilterBy<TDocument>(
    //     this IMongoRepo<TDocument> repo, 
    //     Expression<Func<TDocument, bool>> filter,
    //     int? limit = null) 
    //     where TDocument : MongoDocument
    // {
    //     return repo.NoTrack().FilterBy(filter).Limit(limit).ToEnumerable().ToList();
    // }
    //
    // public static Task<List<TDocument>> FilterByAsync<TDocument>(
    //     this IMongoRepo<TDocument> repo, 
    //     Expression<Func<TDocument, bool>> filter,
    //     int? limit = null) 
    //     where TDocument : MongoDocument
    // {
    //     return repo.NoTrack().FilterBy(filter).Limit(limit).ToListAsync();
    // }
}