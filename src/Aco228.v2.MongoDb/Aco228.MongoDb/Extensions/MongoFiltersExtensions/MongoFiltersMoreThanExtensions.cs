using System.Linq.Expressions;
using Aco228.MongoDb.Extensions.FilterDefinitionExtensions;
using Aco228.MongoDb.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aco228.MongoDb.Extensions.MongoFiltersExtensions;

public static class MongoFiltersMoreThanExtensions
{
    public static LoadSpecification<TDocument, TProjection> Gt<TDocument, TProjection, TKey>(
        this LoadSpecification<TDocument, TProjection> spec, 
        Expression<Func<TDocument, TKey>> selector, 
        TKey? val)
        where TDocument : MongoDocument
        where TProjection : class
    {
        if (val == null) return spec;
        spec.FilterDefinitions.Add(Builders<TDocument>.Filter.Gt(selector, val));
        return spec;
    }
    
    public static LoadSpecification<TDocument, TProjection> NullOrGt<TDocument, TProjection, TKey>(
        this LoadSpecification<TDocument, TProjection> spec, 
        Expression<Func<TDocument, TKey>> selector, 
        TKey? val)
        where TDocument : MongoDocument
        where TProjection : class
    {
        
        var body = selector.Body is UnaryExpression unary ? unary.Operand : selector.Body;
        var name = ((MemberExpression)body).Member.Name;
        
        var filter = Builders<TDocument>.Filter.Or(
            Builders<TDocument>.Filter.Eq(name, BsonNull.Value),
            Builders<TDocument>.Filter.Gt(selector, val)
        );
        
        spec.FilterDefinitions.Add(filter);
        return spec;
    }
    
    public static LoadSpecification<TDocument, TProjection> Gte<TDocument, TProjection, TKey>(
        this LoadSpecification<TDocument, TProjection> spec, 
        Expression<Func<TDocument, TKey>> selector, 
        TKey? val)
        where TDocument : MongoDocument
        where TProjection : class
    {
        if (val == null) return spec;
        spec.FilterDefinitions.Add(Builders<TDocument>.Filter.Gte(selector, val));
        return spec;
    }
    
    
    public static LoadSpecification<TDocument, TProjection> NullOrGte<TDocument, TProjection, TKey>(
        this LoadSpecification<TDocument, TProjection> spec, 
        Expression<Func<TDocument, TKey>> selector, 
        TKey? val)
        where TDocument : MongoDocument
        where TProjection : class
    {
        var body = selector.Body is UnaryExpression unary ? unary.Operand : selector.Body;
        var name = ((MemberExpression)body).Member.Name;
        
        var filter = Builders<TDocument>.Filter.Or(
            Builders<TDocument>.Filter.Eq(name, BsonNull.Value),
            Builders<TDocument>.Filter.Gte(selector, val)
        );
        
        spec.FilterDefinitions.Add(filter);
        return spec;
    }
}