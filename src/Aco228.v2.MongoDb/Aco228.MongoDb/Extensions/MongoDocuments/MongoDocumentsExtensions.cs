using Aco228.Common;
using Aco228.MongoDb.Extensions.RepoExtensions;
using Aco228.MongoDb.Models;
using Aco228.MongoDb.Services;

namespace Aco228.MongoDb.Extensions.MongoDocuments;

public static class MongoDocumentsExtensions
{
    internal static IMongoRepo<TDocument> GetRepo<TDocument>()
        where TDocument : MongoDocument
        => ServiceProviderHelper.GetService<IMongoRepo<TDocument>>()!;

    public static bool HasTrackingAndAnyChanges(this MongoDocumentInternal mongoDocument)
    {
        var obj = mongoDocument.GetTrackingObject();
        if(obj == null) return false;
        return obj.AnyChanges();
    }
    
    public static Task InsertOrUpdateAsync<TDocument>(this TDocument mongoDocument) where TDocument : MongoDocument
        => GetRepo<TDocument>().InsertOrUpdateAsync(mongoDocument);
    
    public static void InsertOrUpdate<TDocument>(this TDocument mongoDocument) where TDocument: MongoDocument
        => GetRepo<TDocument>().InsertOrUpdate(mongoDocument);
    
    public static Task UpdateFieldsAsync<TDocument>(this TDocument mongoDocument) where TDocument: MongoDocument
        => GetRepo<TDocument>().UpdateFieldsAsync(mongoDocument);

    public static Task InsertOrUpdateAsync<TDocument>(this IEnumerable<TDocument> documents)
        where TDocument : MongoDocument
    {
        var mongoDocuments = documents as TDocument[] ?? documents.ToArray();
        if(!mongoDocuments.Any()) return Task.FromResult(true);
        return GetRepo<TDocument>().InsertOrUpdateManyAsync(mongoDocuments);
    }
    

    public static Task UpdateFieldsAsync<TDocument>(this IEnumerable<TDocument> documents)
        where TDocument : MongoDocument
    {
        var mongoDocuments = documents as TDocument[] ?? documents.ToArray();
        if(!mongoDocuments.Any()) return Task.FromResult(true);
        return GetRepo<TDocument>().UpdateFieldsManyAsync(mongoDocuments);
    }
    
    public static void StartTracking<TDocument>(this IEnumerable<TDocument>? documents)
        where TDocument : MongoDocumentInternal
    {
        if (documents == null) return;
        var mongoDocuments = documents as TDocument[] ?? documents.ToArray();
        foreach (var doc in mongoDocuments)
            doc.StartTracking();
    }
}