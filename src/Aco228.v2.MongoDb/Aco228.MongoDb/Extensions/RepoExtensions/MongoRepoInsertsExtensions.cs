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

        if (!isNew && typeof(MongoLite).IsAssignableFrom(typeof(TDocument)))
        {
            repo.UpdateFields(document);
            return;
        }
        
        if(!document.ShouldUpdateIfThereIsTrackingOrChanges())
            return;
        
        document.GetTrackingObject()?.ResetTracking();
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
        repo.GetCollection()!.ReplaceOne(filter, document, new ReplaceOptions { IsUpsert = true });
    }
    
    public static async Task<TDocument> InsertOrUpdateAsync<TDocument>(this IMongoRepo<TDocument> repo, TDocument document)
        where TDocument : MongoDocument
    {
        repo.GuardConfiguration();
        var isNew = document.CheckIfNewAndPrepareForInsert();
        
        if (!isNew && typeof(MongoLite).IsAssignableFrom(typeof(TDocument)))
        {
            return await repo.UpdateFieldsAsync(document);
        }
        
        if(!document.ShouldUpdateIfThereIsTrackingOrChanges())
            return document;
        
        document.GetTrackingObject()?.ResetTracking();
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
        await repo.GetCollection().ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true });
        return document;
    }

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
            
            repo.InsertOrUpdateAsync(document);
            return;
        }
        
        var changedFields = trackObject.GetChangedFields();
        if (!changedFields.Any())
            return;

        foreach (var field in changedFields)
            Console.WriteLine($"Changing {typeof(TDocument).Name}.{field.PropertyName} from {field.OldValue} to {field.NewValue}");

        var updater = Builders<TDocument>.Update;
        var updateList = changedFields.Select(x => updater.Set(x.PropertyName, x.NewValue));
        
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
        var updateList = changedFields.Select(x => updater.Set(x.PropertyName, x.NewValue));
        
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
                
                if (typeof(MongoLite).IsAssignableFrom(typeof(TDocument)))
                {
                    updateFieldsOps.Add(document);
                    continue;
                }
                
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
                
                if (typeof(MongoLite).IsAssignableFrom(typeof(TDocument)))
                {
                    updateFieldsOps.Add(document);
                    continue;
                }
                
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