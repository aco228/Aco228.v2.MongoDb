using Aco228.Common;
using Aco228.MongoDb.Extensions.RepoExtensions;
using Aco228.MongoDb.Models;
using Aco228.MongoDb.Services;

namespace Aco228.MongoDb.Extensions.MongoDocuments;

public static class MongoDocumentsExtensions
{
    public static IMongoRepo<TDocument> GetRepo<TDocument>(this TDocument mongoDocument)
        where TDocument : MongoDocument
        => ServiceProviderHelper.GetService<IMongoRepo<TDocument>>()!;

    public static bool HasTrackingAndAnyChanges(this MongoDocumentInternal mongoDocument)
    {
        var obj = mongoDocument.GetTrackingObject();
        if(obj == null) return false;
        return obj.AnyChanges();
    }
    
    public static Task InsertOrUpdateAsync(this MongoDocument mongoDocument)
        => mongoDocument.GetRepo().InsertOrUpdateAsync(mongoDocument);
    
    public static void InsertOrUpdate(this MongoDocument mongoDocument)
        => mongoDocument.GetRepo().InsertOrUpdate(mongoDocument);
    
    public static Task InsertOrUpdateFieldsAsync(this MongoDocument mongoDocument)
        => mongoDocument.GetRepo().InsertOrUpdateFieldsAsync(mongoDocument);

    public static Task InsertOrUpdateManyAsync<TDocument>(this IEnumerable<TDocument> documents)
        where TDocument : MongoDocument
    {
        var mongoDocuments = documents as TDocument[] ?? documents.ToArray();
        if(!mongoDocuments.Any()) return Task.FromResult(true);
        return mongoDocuments.First().GetRepo().InsertOrUpdateManyAsync(mongoDocuments);
    }

    public static IEnumerable<TDocument> StartTracking<TDocument>(this IEnumerable<TDocument> documents)
        where TDocument : MongoDocument
    {
        var mongoDocuments = documents as TDocument[] ?? documents.ToArray();
        foreach (var doc in mongoDocuments)
            doc.StartTracking();
        
        return mongoDocuments;
    }
}