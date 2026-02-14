using System.Linq.Expressions;
using Aco228.MongoDb.Models;
using MongoDB.Driver;

namespace Aco228.MongoDb.Extensions;

public static class LoadSpecificationExpressionExtensions
{
    private static FilterDefinition<TDocument> NullableConstruct<TDocument, TKey>(
        Expression<Func<TDocument, TKey>> selector, 
        bool append,
        bool isAnd,
        FilterDefinition<TDocument> expression)
    {
        if (!append)
            return expression;
        
        var name = ((MemberExpression)selector.Body).Member.Name;
        if(isAnd)
            return Builders<TDocument>.Filter.And(
                Builders<TDocument>.Filter.Exists(name),
                expression
            );
        else
            return Builders<TDocument>.Filter.Or(
                Builders<TDocument>.Filter.Exists(name),
                expression
            );
    }
    
    public static LoadSpecification<TDocument, TProjection> Equals<TDocument, TProjection, TKey>(
        this LoadSpecification<TDocument, TProjection> spec, 
        Expression<Func<TDocument, TKey>> selector, 
        TKey val)
        where TDocument : MongoDocument
        where TProjection : class
    {
        spec.FilterDefinitions.Add(NullableConstruct(selector, false, false, Builders<TDocument>.Filter.Eq(selector, val)));
        return spec;
    }
    
    public static LoadSpecification<TDocument, TProjection> EqualsOrNull<TDocument, TProjection, TKey>(
        this LoadSpecification<TDocument, TProjection> spec, 
        Expression<Func<TDocument, TKey>> selector, 
        TKey val)
        where TDocument : MongoDocument
        where TProjection : class
    {
        spec.FilterDefinitions.Add(NullableConstruct(selector, true, false, Builders<TDocument>.Filter.Eq(selector, val)));
        return spec;
    }
    
    public static LoadSpecification<TDocument, TProjection> EqualsAndExists<TDocument, TProjection, TKey>(
        this LoadSpecification<TDocument, TProjection> spec, 
        Expression<Func<TDocument, TKey>> selector, 
        TKey val)
        where TDocument : MongoDocument
        where TProjection : class
    {
        spec.FilterDefinitions.Add(NullableConstruct(selector, true, true, Builders<TDocument>.Filter.Eq(selector, val)));
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