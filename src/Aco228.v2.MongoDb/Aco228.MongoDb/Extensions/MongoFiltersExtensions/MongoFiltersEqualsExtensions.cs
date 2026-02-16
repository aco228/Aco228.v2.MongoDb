using System.Linq.Expressions;
using Aco228.MongoDb.Extensions.FilterDefinitionExtensions;
using Aco228.MongoDb.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aco228.MongoDb.Extensions.MongoFiltersExtensions;

public static class MongoFiltersEqualsExtensions
{
    public static LoadSpecification<TDocument, TProjection> Eq<TDocument, TProjection, TKey>(
        this LoadSpecification<TDocument, TProjection> spec, 
        Expression<Func<TDocument, TKey>> selector, 
        TKey val)
        where TDocument : MongoDocument
        where TProjection : class
    {
        spec.FilterDefinitions.Add(Builders<TDocument>.Filter.Eq(selector, val));
        return spec;
    }
    
    public static LoadSpecification<TDocument, TProjection> In<TDocument, TProjection, TKey>(
        this LoadSpecification<TDocument, TProjection> spec, 
        Expression<Func<TDocument, TKey>> selector, 
        IEnumerable<TKey> val)
        where TDocument : MongoDocument
        where TProjection : class
    {
        if (!val.Any())
            return spec;
        
        spec.FilterDefinitions.Add(Builders<TDocument>.Filter.In(selector, val));
        return spec;
    }
    
    public static LoadSpecification<TDocument, TProjection> NullableIn<TDocument, TProjection, TKey>(
        this LoadSpecification<TDocument, TProjection> spec, 
        Expression<Func<TDocument, TKey?>> selector, 
        IEnumerable<TKey> val)
        where TDocument : MongoDocument
        where TProjection : class
        where TKey : struct
    {
        if (!val.Any())
            return spec;
        
        spec.FilterDefinitions.Add(Builders<TDocument>.Filter.In(selector, val.Cast<TKey?>()));
        return spec;
    }
    
    public static LoadSpecification<TDocument, TProjection> NotNull<TDocument, TProjection, TKey>(
        this LoadSpecification<TDocument, TProjection> spec, 
        Expression<Func<TDocument, TKey>> selector)
        where TDocument : MongoDocument
        where TProjection : class
    {
        var body = selector.Body is UnaryExpression unary ? unary.Operand : selector.Body;
        var name = ((MemberExpression)body).Member.Name;
        
        var filter = Builders<TDocument>.Filter.And(
            Builders<TDocument>.Filter.Exists(name),
            Builders<TDocument>.Filter.Ne(name, BsonNull.Value)
        );
        
        spec.FilterDefinitions.Add(filter);
        return spec;
    }
    
    public static LoadSpecification<TDocument, TProjection> NullOrEq<TDocument, TProjection, TKey>(
        this LoadSpecification<TDocument, TProjection> spec, 
        Expression<Func<TDocument, TKey>> selector, 
        TKey val)
        where TDocument : MongoDocument
        where TProjection : class
    {
        var body = selector.Body is UnaryExpression unary ? unary.Operand : selector.Body;
        var name = ((MemberExpression)body).Member.Name;
        
        var filter = Builders<TDocument>.Filter.Or(
            Builders<TDocument>.Filter.Eq(name, BsonNull.Value),
            Builders<TDocument>.Filter.Eq(selector, val)
        );
        
        spec.FilterDefinitions.Add(filter);
        return spec;
    }
    
    public static LoadSpecification<TDocument, TProjection> IsNull<TDocument, TProjection, TKey>(
        this LoadSpecification<TDocument, TProjection> spec, 
        Expression<Func<TDocument, TKey>> selector)
        where TDocument : MongoDocument
        where TProjection : class
    {
        var name = selector.GetName();
        var filter = Builders<TDocument>.Filter.Or(
            Builders<TDocument>.Filter.Exists(name, false),
            Builders<TDocument>.Filter.Eq(name, BsonNull.Value)
        );
        
        spec.FilterDefinitions.Add(filter);
        return spec;
    }
    
    public static LoadSpecification<TDocument, TProjection> NotNullEq<TDocument, TProjection, TKey>(
        this LoadSpecification<TDocument, TProjection> spec, 
        Expression<Func<TDocument, TKey>> selector, 
        TKey val)
        where TDocument : MongoDocument
        where TProjection : class
    {
        var body = selector.Body is UnaryExpression unary ? unary.Operand : selector.Body;
        var name = ((MemberExpression)body).Member.Name;
        
        var filter = Builders<TDocument>.Filter.And(
            Builders<TDocument>.Filter.Exists(name),
            Builders<TDocument>.Filter.Eq(selector, val)
        );
        
        spec.FilterDefinitions.Add(filter);
        return spec;
    }
    
    public static LoadSpecification<TDocument, TProjection> NotNullEq<TDocument, TProjection, TKey>(
        this LoadSpecification<TDocument, TProjection> spec, 
        Expression<Func<TDocument, TKey?>> selector, 
        TKey val)
        where TDocument : MongoDocument
        where TProjection : class
        where TKey : struct
    {
        var field = new ExpressionFieldDefinition<TDocument>(selector);
        var name = selector.GetName();
    
        var filter = Builders<TDocument>.Filter.And(
            Builders<TDocument>.Filter.Exists(field),
            Builders<TDocument>.Filter.Ne(name, BsonNull.Value),
            Builders<TDocument>.Filter.Eq(selector, (TKey?)val)
        );
    
        spec.FilterDefinitions.Add(filter);
        return spec;
    }
    
    public static LoadSpecification<TDocument, TProjection> Exists<TDocument, TProjection>(
        this LoadSpecification<TDocument, TProjection> spec, string fieldName)
        where TDocument : MongoDocument
        where TProjection : class
    {
        spec.FilterDefinitions.Add(Builders<TDocument>.Filter.Exists(fieldName));
        return spec;
    }
}