using MongoDB.Bson;
using MongoDB.Driver;

namespace Aco228.MongoDb.Extensions.FilterDefinitionExtensions;

public static class FilterDefinitionExtensions
{
    public static FilterDefinition<TDocument> PropIsNotNull<TDocument>(
        this string name)
    {
        var filter = Builders<TDocument>.Filter.And(
            Builders<TDocument>.Filter.Exists(name),
            Builders<TDocument>.Filter.Ne(name, BsonNull.Value)
        );
        
        return filter;
    }
    
    public static FilterDefinition<TDocument> PropIsNull<TDocument>(
        this string name)
    {
        var filter = Builders<TDocument>.Filter.Or(
            Builders<TDocument>.Filter.Exists(name, false),
            Builders<TDocument>.Filter.Ne(name, BsonNull.Value)
        );
        
        return filter;
    }
    
}