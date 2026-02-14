using System.Linq.Expressions;
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
    
    public static LoadSpecification<TDocument, TProjection> NullOrEq<TDocument, TProjection, TKey>(
        this LoadSpecification<TDocument, TProjection> spec, 
        Expression<Func<TDocument, TKey>> selector, 
        TKey val)
        where TDocument : MongoDocument
        where TProjection : class
    {
        spec.FilterDefinitions.NullableConstruct(selector, true, false, Builders<TDocument>.Filter.Eq(selector, val));
        spec.FilterDefinitions.NullableConstruct(selector, true, false, Builders<TDocument>.Filter.Eq(selector, val));
        return spec;
    }
    
    public static LoadSpecification<TDocument, TProjection> ExistsAndEq<TDocument, TProjection, TKey>(
        this LoadSpecification<TDocument, TProjection> spec, 
        Expression<Func<TDocument, TKey>> selector, 
        TKey val)
        where TDocument : MongoDocument
        where TProjection : class
    {
        spec.FilterDefinitions.NullableConstruct(selector, true, true, Builders<TDocument>.Filter.Eq(selector, val));
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