using Aco228.MongoDb.Models;
using Aco228.MongoDb.Services;
using MongoDB.Driver;

namespace Aco228.MongoDb.Extensions.RepoExtensions;

public static class MongoRepoInsertFieldsExtensions
{
    public static void UpdateFields<TDocument>(this IMongoRepo<TDocument> repo, TDocument document)
        where TDocument : MongoDocument
    {
        repo.GuardConfiguration();
        if (document.CheckIfNewAndPrepareForInsert())
        {
            repo.InsertOrUpdate(document);
            return;
        }
        
        var trackObject = document.GetTrackingObject();
        if (trackObject == null)
        {
            if(typeof(MongoLite).IsAssignableFrom(typeof(TDocument)))
                throw new InvalidOperationException("MongoLite must have tracking object");
            
            repo.InsertOrUpdate(document);
            return;
        }
        
        var changedFields = trackObject.GetChangedFields();
        if (!changedFields.Any())
            return;

        foreach (var field in changedFields)
            Console.WriteLine($"Changing {typeof(TDocument).Name}.{field.PropertyName} from {field.OldValue} to {field.NewValue}");

        var updater = Builders<TDocument>.Update;
        var updateList = changedFields.Select(x => updater.Set(x.PropertyName, x.NewValue)).ToList();
        updateList.Add(updater.Set(x => x.UpdatedUtc, DT.GetUnix()));
        
        repo.GetCollection().UpdateOne(Builders<TDocument>.Filter.Eq(x => x.Id, document.Id), updater.Combine(updateList));
        trackObject.ResetTracking();
    }
    

    public static async Task<TDocument> UpdateFieldsAsync<TDocument>(this IMongoRepo<TDocument> repo, TDocument document)
        where TDocument : MongoDocument
    {
        repo.GuardConfiguration();
        if (document.CheckIfNewAndPrepareForInsert())
        {
            await repo.InsertOrUpdateAsync(document);
            return document;
        }
        
        var trackObject = document.GetTrackingObject();
        if (trackObject == null)
        {
            if(typeof(MongoLite).IsAssignableFrom(typeof(TDocument)))
                throw new InvalidOperationException("MongoLite must have tracking object");
            
            await repo.InsertOrUpdateAsync(document);
            return document;
        }
        
        var changedFields = trackObject.GetChangedFields();
        if (!changedFields.Any())
            return document;

        foreach (var field in changedFields)
            Console.WriteLine($"Changing {typeof(TDocument).Name}.{field.PropertyName} from {field.OldValue} to {field.NewValue}");

        var updater = Builders<TDocument>.Update;
        var updateList = changedFields.Select(x => updater.Set(x.PropertyName, x.NewValue)).ToList();
        updateList.Add(updater.Set(x => x.UpdatedUtc, DT.GetUnix()));
        
        await repo.GetCollection().UpdateOneAsync(Builders<TDocument>.Filter.Eq(x => x.Id, document.Id), updater.Combine(updateList));
        trackObject.ResetTracking();
        return document;
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
            var isNew = document.CheckIfNewAndPrepareForInsert();
            
            if (!isNew && trackObject == null && typeof(MongoLite).IsAssignableFrom(typeof(TDocument)))
                throw new InvalidOperationException("MongoLite must have tracking object");
            
            if (isNew || trackObject == null)
            {
                inserts.Add(document);
                continue;
            }    
            
            var changedFields = trackObject.GetChangedFields();
            if (!changedFields.Any())
                continue;

            var updateList = changedFields.Select(x => updater.Set(x.PropertyName, x.NewValue)).ToList();
            updateList.Add(updater.Set(x => x.UpdatedUtc, DT.GetUnix()));
            
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
            operations.Add(new UpdateOneModel<TDocument>(filter, updater.Combine(updateList)));
            trackObject.ResetTracking();
        }
        
        if(inserts.Any())
            await repo.InsertOrUpdateManyAsync(inserts);
        
        if(operations.Any())
            await repo.GetCollection().BulkWriteAsync(operations, new() { IsOrdered = false });
    }
}