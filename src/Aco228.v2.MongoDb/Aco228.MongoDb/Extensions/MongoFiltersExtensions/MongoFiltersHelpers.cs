using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aco228.MongoDb.Extensions.MongoFiltersExtensions;

internal static class MongoFiltersHelpers
{
    public static List<FilterDefinition<TDocument>> NullableConstruct<TDocument, TKey>(
        this List<FilterDefinition<TDocument>> filters,
        Expression<Func<TDocument, TKey>> selector,
        bool append,
        bool mustExist,
        FilterDefinition<TDocument> expression
    )
    {
        if (!append)
        {
            filters.Add(expression);
            return filters;
        }
        
        var body = selector.Body is UnaryExpression unary ? unary.Operand : selector.Body;
        var name = ((MemberExpression)body).Member.Name;
        
        if (mustExist)
        {
            // Field must exist AND satisfy the expression
            filters.Add(Builders<TDocument>.Filter.And(
                Builders<TDocument>.Filter.Exists(name),
                Builders<TDocument>.Filter.Ne(name, BsonNull.Value),
                expression
            ));
        }
        else
        {
            // Field missing OR satisfies the expression
            filters.Add(Builders<TDocument>.Filter.Or(
                Builders<TDocument>.Filter.Eq(name, BsonNull.Value),
                expression
            ));
        }

        return filters;
    }
}