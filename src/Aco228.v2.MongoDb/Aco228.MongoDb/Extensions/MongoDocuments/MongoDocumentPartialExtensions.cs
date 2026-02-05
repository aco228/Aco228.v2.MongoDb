using Aco228.MongoDb.Models;
using MongoDB.Driver;

namespace Aco228.MongoDb.Extensions.MongoDocuments;

public static class MongoDocumentPartialExtensions
{
    public static Task UpdateAsync<TDocument>(this MongoProjection<TDocument> document)
        where TDocument : MongoDocument
    => new[] {document}.UpdateAsync();
    
    public static async Task UpdateAsync<TDocument>(this IEnumerable<MongoProjection<TDocument>> documents)
        where TDocument : MongoDocument
    {
        var repo = MongoDocumentsExtensions.GetRepo<TDocument>();
        repo.GuardConfiguration();
        
        var updater = Builders<TDocument>.Update;
        var operations = new List<WriteModel<TDocument>>();
        foreach (var document in documents)
        {
            var trackObject = document.GetTrackingObject();
            if (trackObject == null)
                throw new InvalidOperationException("Tracking object is null");
            
            var changedFields = trackObject.GetChangedFields();
            if (!changedFields.Any())
                continue;

            var updateList = changedFields.Select(x => updater.Set(x.PropertyName, x.NewValue));
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
            operations.Add(new UpdateOneModel<TDocument>(filter, updater.Combine(updateList)));
            trackObject.ResetTracking();
        }
        
        if(operations.Any())
            await repo.GetCollection().BulkWriteAsync(operations, new() { IsOrdered = false });  
    }
    
}