using System.Linq.Expressions;
using Aco228.MongoDb.Extensions.FilterDefinitionExtensions;
using Aco228.MongoDb.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aco228.MongoDb.Extensions.MongoFiltersExtensions;

public static class MongoFiltersStringExtensions
{
    public static LoadSpecification<TDocument, TProjection> RegexString<TDocument, TProjection, TKey>(
        this LoadSpecification<TDocument, TProjection> spec, 
        Expression<Func<TDocument, TKey>> selector,
        string? regex)
        where TDocument : MongoDocument
        where TProjection : class
    {
        if(string.IsNullOrEmpty(regex)) 
            return spec;
        
        var field = new ExpressionFieldDefinition<TDocument>(selector);
        var filter = Builders<TDocument>.Filter.Regex(field, new BsonRegularExpression(regex, "i"));
        
        spec.FilterDefinitions.Add(filter);
        return spec;
    }
}