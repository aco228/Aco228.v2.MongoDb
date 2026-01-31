using Aco228.MongoDb.Models;
using Aco228.MongoDb.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aco228.MongoDb.Extensions.RepoExtensions;

public static class MongoRepoDeleteExtensions
{
    //
    //  SINGLE DELETION
    //
    
    public static void DeleteById<TDocument>(this IMongoRepo<TDocument> repo, ObjectId objectId)
        where TDocument : MongoDocument
    {
        repo.GuardConfiguration();
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, objectId);
        repo.GetCollection().FindOneAndDelete(filter);
    }
    
    public static Task DeleteByIdAsync<TDocument>(this IMongoRepo<TDocument> repo, ObjectId objectId)
        where TDocument : MongoDocument
    {
        repo.GuardConfiguration();
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, objectId);
        return repo.GetCollection().FindOneAndDeleteAsync(filter);
    }
    
    public static void DeleteById<TDocument>(this IMongoRepo<TDocument> repo, string documentId) where TDocument : MongoDocument => repo.DeleteById( new ObjectId(documentId));
    public static Task DeleteByIdAsync<TDocument>(this IMongoRepo<TDocument> repo, string documentId) where TDocument : MongoDocument => repo.DeleteByIdAsync(new ObjectId(documentId));
    public static void Delete<TDocument>(this IMongoRepo<TDocument> repo, TDocument document) where TDocument : MongoDocument => repo.DeleteById(document.Id);
    public static Task DeleteAsync<TDocument>(this IMongoRepo<TDocument> repo, TDocument document) where TDocument : MongoDocument => repo.DeleteByIdAsync(document.Id);
    
    
    
    //
    //  MANY DELETION
    //

    public static void DeleteMany<TDocument>(this IMongoRepo<TDocument> repo, IEnumerable<TDocument> documents)
        where TDocument : MongoDocument
    {
        var mongoDocuments = documents as TDocument[] ?? documents.ToArray();
        
        if (!mongoDocuments.Any()) return;
        repo.GuardConfiguration();
        var ids = mongoDocuments.Select(d => d.Id).ToList();
        var filter = Builders<TDocument>.Filter.In(d => d.Id, ids);
        repo.GetCollection().DeleteMany(filter);
    }
    

    public static Task DeleteManyAsync<TDocument>(this IMongoRepo<TDocument> repo, IEnumerable<TDocument> documents)
        where TDocument : MongoDocument
    {
        var mongoDocuments = documents as TDocument[] ?? documents.ToArray();
        
        if (!mongoDocuments.Any()) return Task.FromResult(true);
        repo.GuardConfiguration();
        var ids = mongoDocuments.Select(d => d.Id).ToList();
        var filter = Builders<TDocument>.Filter.In(d => d.Id, ids);
        return repo.GetCollection().DeleteManyAsync(filter);
    }
    
    
}