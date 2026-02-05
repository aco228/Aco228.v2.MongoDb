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
        
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
        repo.GetCollection()!.ReplaceOne(filter, document, new ReplaceOptions { IsUpsert = true });
    }
    
    public static Task InsertOrUpdateAsync<TDocument>(this IMongoRepo<TDocument> repo, TDocument document)
        where TDocument : MongoDocument
    {
        repo.GuardConfiguration();
        document.CheckIfNewAndPrepareForInsert();
        
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
        return repo.GetCollection().ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true });
    }

    public static async Task InsertOrUpdateFieldsAsync<TDocument>(this IMongoRepo<TDocument> repo, TDocument document)
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
            throw new InvalidOperationException("Document is not tracked");
        
        var changedFields = trackObject.GetChangedFields();
        if (!changedFields.Any())
            return;

        var updater = Builders<TDocument>.Update;
        var updateList = changedFields.Select(x => updater.Set(x.PropertyName, x.NewValue));
        
        await repo.GetCollection().UpdateOneAsync(Builders<TDocument>.Filter.Eq(x => x.Id, document.Id), updater.Combine(updateList));
        trackObject.ClearTracking().StartTracking();
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
                var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
                operations.Add(new ReplaceOneModel<TDocument>(filter, document));
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
                var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
                operations.Add(new ReplaceOneModel<TDocument>(filter, document));
            }
        }

        if(!operations.Any())
            return Task.FromResult(0);
        
        return repo.GetCollection()!.BulkWriteAsync(operations, new() { IsOrdered = false });
    }
}