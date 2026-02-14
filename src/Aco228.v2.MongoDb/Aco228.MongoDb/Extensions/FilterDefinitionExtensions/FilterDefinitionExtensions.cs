using System.Linq.Expressions;
using Aco228.MongoDb.Models;
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
    
    public static List<FilterDefinition<TDocument>> PropIsEq<TDocument, TKey>(
        this List<FilterDefinition<TDocument>> filterBody,
        Expression<Func<TDocument, TKey>> selector, TKey val)
    {
        filterBody.Add(Builders<TDocument>.Filter.Eq(selector, val));
        return filterBody;
    }
    
    public static List<FilterDefinition<TDocument>> PropIsNullOrEq<TDocument, TKey>(
        this List<FilterDefinition<TDocument>> filterBody,
        Expression<Func<TDocument, TKey>> selector, TKey val)
    {
        var nullProp = selector.GetName().PropIsNull<TDocument>();
        filterBody.Add(Builders<TDocument>.Filter.Or(nullProp, Builders<TDocument>.Filter.Eq(selector, val)));
        return filterBody;
    }
    
    public static List<FilterDefinition<TDocument>> PropExistsAndEq<TDocument, TKey>(
        this List<FilterDefinition<TDocument>> filterBody,
        Expression<Func<TDocument, TKey>> selector, TKey val)
    {
        var nullProp = selector.GetName().PropIsNotNull<TDocument>();
        filterBody.Add(Builders<TDocument>.Filter.Or(nullProp, Builders<TDocument>.Filter.Eq(selector, val)));
        return filterBody;
    }
    
}