using System.Linq.Expressions;
using Aco228.MongoDb.Extensions.FilterDefinitionExtensions;
using Aco228.MongoDb.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aco228.MongoDb.Extensions.MongoFiltersExtensions;

public static class MongoFiltersLessThanExtensions
{
    public static LoadSpecification<TDocument, TProjection> Lt<TDocument, TProjection, TKey>(
        this LoadSpecification<TDocument, TProjection> spec, 
        Expression<Func<TDocument, TKey>> selector, 
        TKey? val)
        where TDocument : MongoDocument
        where TProjection : class
    {
        if (val == null) return spec;
        spec.FilterDefinitions.Add(Builders<TDocument>.Filter.Lt(selector, val));
        return spec;
    }
    
    public static LoadSpecification<TDocument, TProjection> Lte<TDocument, TProjection, TKey>(
        this LoadSpecification<TDocument, TProjection> spec, 
        Expression<Func<TDocument, TKey>> selector, 
        TKey? val)
        where TDocument : MongoDocument
        where TProjection : class
    {
        if (val == null) return spec;
        spec.FilterDefinitions.Add(Builders<TDocument>.Filter.Lte(selector, val));
        return spec;
    }
}