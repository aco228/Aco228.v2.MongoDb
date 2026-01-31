using Aco228.MongoDb.Extensions.RepoExtensions;
using Aco228.MongoDb.Models;
using Aco228.MongoDb.Services;
using MongoDB.Driver;

namespace Aco228.MongoDb.Extensions;

public static class MongoLoadBySpecificationExtensions
{
    public static long Count<TDocument, TProjection>(this IMongoRepo<TDocument> repo,
        LoadSpecification<TDocument, TProjection> spec)
        where TDocument : MongoDocument
        where TProjection : class
        => spec.SetRepo(repo).GetCursor().CountDocuments();

    public static Task<long> CountAsync<TDocument, TProjection>(this IMongoRepo<TDocument> repo,
        LoadSpecification<TDocument, TProjection> spec)
        where TDocument : MongoDocument
        where TProjection : class
        => spec.SetRepo(repo).GetCursor().CountDocumentsAsync();

    public static TProjection? FirstOrDefault<TDocument, TProjection>(this IMongoRepo<TDocument> repo,
        LoadSpecification<TDocument, TProjection> spec)
        where TDocument : MongoDocument
        where TProjection : class
        => spec.SetRepo(repo).GetCursor().FirstOrDefault().ProjectSingle(spec);

    public static async Task<TProjection>? FirstOrDefaultAsync<TDocument, TProjection>(this IMongoRepo<TDocument> repo,
        LoadSpecification<TDocument, TProjection> spec)
        where TDocument : MongoDocument
        where TProjection : class
    {
        using var cursor = await spec.SetRepo(repo).GetCursorAsync();
        return (await cursor.FirstOrDefaultAsync()).ProjectSingle(spec);
    }

    public static List<TProjection> ToList<TDocument, TProjection>(this IMongoRepo<TDocument> repo,
        LoadSpecification<TDocument, TProjection> spec)
        where TDocument : MongoDocument
        where TProjection : class
        => spec.SetRepo(repo).GetCursor().ToList().ProjectList(spec);

    public static IEnumerable<TProjection> ToEnumerable<TDocument, TProjection>(this IMongoRepo<TDocument> repo,
        LoadSpecification<TDocument, TProjection> spec)
        where TDocument : MongoDocument
        where TProjection : class
        => spec.SetRepo(repo).GetCursor().ToEnumerable().ProjectEnumerable(spec);

    public static async Task<List<TProjection>> ToListAsync<TDocument, TProjection>(this IMongoRepo<TDocument> repo,
        LoadSpecification<TDocument, TProjection> spec)
        where TDocument : MongoDocument
        where TProjection : class
    {
        using var cursor = await spec.SetRepo(repo).GetCursorAsync();
        return (await cursor.ToListAsync()).ProjectList(spec);
    }
}

public static class MongoRepoLoadBySpecificationExtensions
{
    public static long Count<TDocument, TProjection>(this LoadSpecification<TDocument, TProjection> spec)
        where TDocument : MongoDocument
        where TProjection : class
        => spec.GetCursor().CountDocuments();

    public static Task<long> CountAsync<TDocument, TProjection>(this LoadSpecification<TDocument, TProjection> spec)
        where TDocument : MongoDocument
        where TProjection : class
        => spec.GetCursor().CountDocumentsAsync();

    public static TProjection? FirstOrDefault<TDocument, TProjection>(this LoadSpecification<TDocument, TProjection> spec)
        where TDocument : MongoDocument
        where TProjection : class
        => spec.GetCursor().FirstOrDefault().ProjectSingle(spec);

    public static async Task<TProjection>? FirstOrDefaultAsync<TDocument, TProjection>(
        this LoadSpecification<TDocument, TProjection> spec)
        where TDocument : MongoDocument
        where TProjection : class
    {
        using var cursor = await spec.GetCursorAsync();
        return (await cursor.FirstOrDefaultAsync()).ProjectSingle(spec);
    }

    public static List<TProjection> ToList<TDocument, TProjection>(this LoadSpecification<TDocument, TProjection> spec)
        where TDocument : MongoDocument
        where TProjection : class
        => spec.GetCursor().ToList().ProjectList(spec);

    public static IEnumerable<TProjection> ToEnumerable<TDocument, TProjection>(
        this LoadSpecification<TDocument, TProjection> spec)
        where TDocument : MongoDocument
        where TProjection : class
        => spec.GetCursor().ToEnumerable().ProjectEnumerable(spec);

    public static async Task<List<TProjection>> ToListAsync<TDocument, TProjection>(
        this LoadSpecification<TDocument, TProjection> spec)
        where TDocument : MongoDocument
        where TProjection : class
    {
        using var cursor = await spec.GetCursorAsync();
        return (await cursor.ToListAsync()).ProjectList(spec);
    }

    // public static async IAsyncEnumerable<TProjection> LoadInBatchesAsync<TDocument, TProjection>(
    //     this LoadSpecification<TDocument, TProjection> spec, int batchSize = 50)
    //     where TDocument : MongoDocument
    //     where TProjection : class
    // {
    //     if (spec.Repo == null) throw new InvalidOperationException("Repo not present in specification");
    //
    //     using var cursor = await spec.GetCursorAsync(batchSize: batchSize);
    //     int max = 15000;
    //     List<TDocument>? current = null;
    //
    //     for (int i = 0; i < max; i++)
    //     {
    //         var task = cursor.MoveNextAsync();
    //         if (current != null && current.Any())
    //         {
    //             foreach (var elem in current)
    //                 yield return elem;
    //
    //             current = null;
    //         }
    //
    //         await task;
    //
    //         if (task.Result == false)
    //             break;
    //
    //         current = cursor.Current.ToList();
    //     }
    // }
}