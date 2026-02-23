using Aco228.MongoDb.Extensions.MongoDocuments;
using Aco228.MongoDb.Models;
using Aco228.MongoDb.Services;
using MongoDB.Driver;

namespace Aco228.MongoDb.Extensions.RepoExtensions;

public static class MongoRepoInsertsExtensions
{
    public static void InsertOrUpdate<TDocument>(this IMongoRepo<TDocument> repo, TDocument document)
        where TDocument : MongoDocument
    {
        repo.GuardConfiguration();
        var isNew = document.CheckIfNewAndPrepareForInsert();

        if (!isNew && typeof(MongoLite).IsAssignableFrom(typeof(TDocument)) && document.IgnoreTrackingObject == false)
        {
            repo.UpdateFields(document);
            return;
        }
        
        if(!document.ShouldUpdateIfThereIsTrackingOrChanges())
            return;
        
        document.GetTrackingObject()?.ResetTracking();
        MongoDocumentExtensions.SetDocumentDefaultValues(document);
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
        repo.GetCollection()!.ReplaceOne(filter, document, new ReplaceOptions { IsUpsert = true });
    }
    
    public static async Task<TDocument> InsertOrUpdateAsync<TDocument>(this IMongoRepo<TDocument> repo, TDocument document)
        where TDocument : MongoDocument
    {
        repo.GuardConfiguration();
        var isNew = document.CheckIfNewAndPrepareForInsert();
        
        if (!isNew && typeof(MongoLite).IsAssignableFrom(typeof(TDocument)) && document.IgnoreTrackingObject == false)
        {
            return await repo.UpdateFieldsAsync(document);
        }
        
        if(!document.ShouldUpdateIfThereIsTrackingOrChanges())
            return document;
        
        document.GetTrackingObject()?.ResetTracking();
        
        MongoDocumentExtensions.SetDocumentDefaultValues(document);
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
        await repo.GetCollection().ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true });
        return document;
    }


    public static void InsertOrUpdateMany<TDocument>(this IMongoRepo<TDocument> repo, IEnumerable<TDocument> documents)
        where TDocument : MongoDocument
    {
        repo.GuardConfiguration();
        var mongoDocuments = documents as TDocument[] ?? documents.ToArray();
        if(!mongoDocuments.Any())
            return;

        var updateFieldsOps = new List<TDocument>();
        var operations = new List<WriteModel<TDocument>>();
        foreach (var document in mongoDocuments)
        {
            if (document.CheckIfNewAndPrepareForInsert())
                operations.Add(new InsertOneModel<TDocument>(document));
            else
            {
                if (!document.ShouldUpdateIfThereIsTrackingOrChanges())
                    continue;
                
                if (typeof(MongoLite).IsAssignableFrom(typeof(TDocument)) &&  document.IgnoreTrackingObject == false)
                {
                    updateFieldsOps.Add(document);
                    continue;
                }
                
                MongoDocumentExtensions.SetDocumentDefaultValues(document);
                var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
                operations.Add(new ReplaceOneModel<TDocument>(filter, document));
                document.GetTrackingObject()?.ResetTracking();
            }
        }
        
        if(updateFieldsOps.Any())
            throw new InvalidOperationException("MongoLite must have tracking object");

        if(!operations.Any())
            return;
        
        var result = repo.GetCollection().BulkWrite(operations, new() { IsOrdered = false });
    }

    public static async Task InsertOrUpdateManyAsync<TDocument>(this IMongoRepo<TDocument> repo, IEnumerable<TDocument> documents)
        where TDocument : MongoDocument
    {
        repo.GuardConfiguration();
        var mongoDocuments = documents as TDocument[] ?? documents.ToArray();
        if(!mongoDocuments.Any())
            return;
        
        var operations = new List<WriteModel<TDocument>>();
        var updateFieldsOps = new List<TDocument>();
        
        foreach (var document in mongoDocuments)
        {
            if (document.CheckIfNewAndPrepareForInsert())
                operations.Add(new InsertOneModel<TDocument>(document));
            else
            {
                if (!document.ShouldUpdateIfThereIsTrackingOrChanges())
                    continue;
                
                if (typeof(MongoLite).IsAssignableFrom(typeof(TDocument)) && document.IgnoreTrackingObject == false)
                {
                    updateFieldsOps.Add(document);
                    continue;
                }
                
                MongoDocumentExtensions.SetDocumentDefaultValues(document);
                var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
                operations.Add(new ReplaceOneModel<TDocument>(filter, document));
                document.GetTrackingObject()?.ResetTracking();
            }
        }
        
        if(updateFieldsOps.Any())
            await repo.UpdateFieldsManyAsync( updateFieldsOps);

        if (!operations.Any())
            return;
        
        await repo.GetCollection()!.BulkWriteAsync(operations, new() { IsOrdered = false });
    }
}