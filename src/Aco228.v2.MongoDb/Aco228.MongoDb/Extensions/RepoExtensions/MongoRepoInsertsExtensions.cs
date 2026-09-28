using Aco228.MongoDb.Constants;
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
        
        document.BeforeSave();
        if(!document.ShouldUpdateIfThereIsTrackingOrChanges())
            return;
        
        document.GetTrackingObject()?.ResetTracking();
        MongoDocumentExtensions.SetDocumentDefaultValues(document);
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
        repo.GetCollection()!.ReplaceOne(filter, document, new ReplaceOptions { IsUpsert = true });
    }


    public static async Task<bool> TryInsertOrUpdateAsync<TDocument>(this IMongoRepo<TDocument> repo, TDocument document)
        where TDocument : MongoDocument
    {
        try
        {
            await InsertOrUpdateAsync(repo, document);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    /// <summary>
    /// Bulk version of <see cref="TryInsertOrUpdateAsync{TDocument}"/>: inserts the new documents and updates the
    /// existing ones in one unordered bulk write. Documents rejected by a unique index (duplicate key) are skipped,
    /// and the rest are still written. Returns false only when something else failed, never throws.
    /// </summary>
    public static async Task<bool> TryInsertOrUpdateManyAsync<TDocument>(this IMongoRepo<TDocument> repo, IEnumerable<TDocument> documents)
        where TDocument : MongoDocument
    {
        try
        {
            await InsertOrUpdateManyAsync(repo, documents);
            return true;
        }
        catch (MongoBulkWriteException e) when (e.WriteConcernError == null
                                                && e.WriteErrors.All(x => x.Category == ServerErrorCategory.DuplicateKey))
        {
            // Unordered: every document without a conflict was written, only the duplicates were skipped.
            return true;
        }
        catch (Exception)
        {
            return false;
        }
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
        
        document.BeforeSave();
        if(!document.ShouldUpdateIfThereIsTrackingOrChanges())
            return document;
        
        document.GetTrackingObject()?.ResetTracking();
        
        MongoDocumentExtensions.SetDocumentDefaultValues(document);
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
        
        await PollyConfiguration.Async.ExecuteAsync(async () =>
        {
            await repo.GetCollection().ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true });
        });
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
            document.BeforeSave();
            if (document.CheckIfNewAndPrepareForInsert())
            {
                MongoDocumentExtensions.SetDocumentDefaultValues(document);
                operations.Add(new InsertOneModel<TDocument>(document));
            }
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
        
        PollyConfiguration.Sync.Execute(() =>
        {
            var result = repo.GetCollection().BulkWrite(operations, new() { IsOrdered = false });
        });
    }

    public static async Task InsertOrUpdateManyInBatchesAsync<TDocument>(
        this IMongoRepo<TDocument> repo,
        IEnumerable<TDocument> documents,
        int perBatch = 50) where TDocument : MongoDocument
    {
        var current = new List<TDocument>();
        foreach (var document in documents)
        {
            document.BeforeSave();
            current.Add(document);
            if (current.Count >= perBatch)
            {
                await repo.InsertOrUpdateManyAsync(current);
                current.Clear();
            }
        }
        
        if (current.Any())
            await repo.InsertOrUpdateManyAsync(current);
    }

    public static async Task InsertOrUpdateManyAsync<TDocument>(this IMongoRepo<TDocument> repo, IEnumerable<TDocument> documents) where TDocument : MongoDocument
    {
        repo.GuardConfiguration();
        var mongoDocuments = documents as TDocument[] ?? documents.ToArray();
        if(!mongoDocuments.Any())
            return;
        
        var operations = new List<WriteModel<TDocument>>();
        var updateFieldsOps = new List<TDocument>();
        
        foreach (var document in mongoDocuments)
        {
            document.BeforeSave();
            if (document.CheckIfNewAndPrepareForInsert())
            {
                MongoDocumentExtensions.SetDocumentDefaultValues(document);
                operations.Add(new InsertOneModel<TDocument>(document));
            }
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

        await PollyConfiguration.Async.ExecuteAsync(async () =>
        {
            await repo.GetCollection()!.BulkWriteAsync(operations, new() { IsOrdered = false });
        });
    }
}