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
        document.CheckIfNewAndPrepareForInsert();
        
        if(!document.ShouldUpdateIfThereIsTrackingOrChanges())
            return;
        
        document.GetTrackingObject()?.ResetTracking();
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
        repo.GetCollection()!.ReplaceOne(filter, document, new ReplaceOptions { IsUpsert = true });
    }
    
    public static Task InsertOrUpdateAsync<TDocument>(this IMongoRepo<TDocument> repo, TDocument document)
        where TDocument : MongoDocument
    {
        repo.GuardConfiguration();
        document.CheckIfNewAndPrepareForInsert();
        
        if(!document.ShouldUpdateIfThereIsTrackingOrChanges())
            return Task.FromResult(true);
        
        document.GetTrackingObject()?.ResetTracking();
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
        return repo.GetCollection().ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true });
    }

    public static async Task UpdateFieldsAsync<TDocument>(this IMongoRepo<TDocument> repo, TDocument document)
        where TDocument : MongoDocument
    {
        repo.GuardConfiguration();
        if (document.CheckIfNewAndPrepareForInsert())
        {
            await repo.InsertOrUpdateAsync( document);
            return;
        }
        
        var trackObject = document.GetTrackingObject();
        if (trackObject == null)
        {
            await repo.InsertOrUpdateAsync( document);
            return;
        }
        
        var changedFields = trackObject.GetChangedFields();
        if (!changedFields.Any())
            return;

        foreach (var field in changedFields)
            Console.WriteLine($"Changing {field.PropertyName} from {field.OldValue} to {field.NewValue}");

        var updater = Builders<TDocument>.Update;
        var updateList = changedFields.Select(x => updater.Set(x.PropertyName, x.NewValue));
        
        await repo.GetCollection().UpdateOneAsync(Builders<TDocument>.Filter.Eq(x => x.Id, document.Id), updater.Combine(updateList));
        trackObject.ResetTracking();
    }
    

    public static async Task UpdateFieldsManyAsync<TDocument>(this IMongoRepo<TDocument> repo, IEnumerable<TDocument> documents)
        where TDocument : MongoDocument
    {
        repo.GuardConfiguration();
        var updater = Builders<TDocument>.Update;
        var inserts = new List<TDocument>();
        var operations = new List<WriteModel<TDocument>>();
        foreach (var document in documents)
        {
            var trackObject = document.GetTrackingObject();
            if (document.CheckIfNewAndPrepareForInsert() || trackObject == null)
            {
                inserts.Add(document);
                continue;
            }    
            
            var changedFields = trackObject.GetChangedFields();
            if (!changedFields.Any())
                continue;

            var updateList = changedFields.Select(x => updater.Set(x.PropertyName, x.NewValue));
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
            operations.Add(new UpdateOneModel<TDocument>(filter, updater.Combine(updateList)));
            trackObject.ResetTracking();
        }
        
        if(inserts.Any())
            await repo.InsertOrUpdateManyAsync(inserts);
        
        if(operations.Any())
            await repo.GetCollection().BulkWriteAsync(operations, new() { IsOrdered = false });
    }

    public static void InsertOrUpdateMany<TDocument>(this IMongoRepo<TDocument> repo, IEnumerable<TDocument> documents)
        where TDocument : MongoDocument
    {
        repo.GuardConfiguration();
        var mongoDocuments = documents as TDocument[] ?? documents.ToArray();
        if(!mongoDocuments.Any())
            return;
        
        var operations = new List<WriteModel<TDocument>>();
        foreach (var document in mongoDocuments)
        {
            if (document.CheckIfNewAndPrepareForInsert())
                operations.Add(new InsertOneModel<TDocument>(document));
            else
            {
                if (!document.ShouldUpdateIfThereIsTrackingOrChanges())
                    continue;
                
                var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
                operations.Add(new ReplaceOneModel<TDocument>(filter, document));
                document.GetTrackingObject()?.ResetTracking();
            }
        }

        if(!operations.Any())
            return;
        
        var result = repo.GetCollection().BulkWrite(operations, new() { IsOrdered = false });
    }

    public static Task InsertOrUpdateManyAsync<TDocument>(this IMongoRepo<TDocument> repo, IEnumerable<TDocument> documents)
        where TDocument : MongoDocument
    {
        repo.GuardConfiguration();
        var mongoDocuments = documents as TDocument[] ?? documents.ToArray();
        if(!mongoDocuments.Any())
            return Task.FromResult(0);
        
        var operations = new List<WriteModel<TDocument>>();
        foreach (var document in mongoDocuments)
        {
            if (document.CheckIfNewAndPrepareForInsert())
                operations.Add(new InsertOneModel<TDocument>(document));
            else
            {
                if (!document.ShouldUpdateIfThereIsTrackingOrChanges())
                    continue;
                
                var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
                operations.Add(new ReplaceOneModel<TDocument>(filter, document));
                document.GetTrackingObject()?.ResetTracking();
            }
        }

        if(!operations.Any())
            return Task.FromResult(0);
        
        return repo.GetCollection()!.BulkWriteAsync(operations, new() { IsOrdered = false });
    }
}