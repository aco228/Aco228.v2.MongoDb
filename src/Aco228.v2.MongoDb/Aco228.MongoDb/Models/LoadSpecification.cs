using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aco228.MongoDb.Models;

public class LoadSpecification<TDocument>
    where TDocument : MongoDocument
{
    private List<Expression<Func<TDocument, bool>>> _expressions = new();
    private SortDefinition<TDocument>? _sort;
    private int? _limit;
    private int? _skip;
    
    public LoadSpecification<TDocument> FilterBy(Expression<Func<TDocument, bool>>? filter)
    {
        if (filter == null) return this;
        _expressions.Add(filter);
        return this;
    }
    
    public LoadSpecification<TDocument> Limit(int? limit)
    {
        _limit = limit;
        return this;
    }

    public LoadSpecification<TDocument> Skip(int? skip)
    {
        _skip = skip;
        return this;
    }
    
    public LoadSpecification<TDocument> AfterDocument(TDocument lastObject)
    {
        FilterBy(x => x.Id.CompareTo(lastObject.Id) > 0);
        return this;
    }
    public LoadSpecification<TDocument> AfterId(ObjectId lastId)
    {
        FilterBy(x => x.Id.CompareTo(lastId) > 0);
        return this;
    }

    public LoadSpecification<TDocument> OrderByPropertyName(OrderDirection orderDirection, string propertyName)
    {
        if(orderDirection == OrderDirection.ASC)
            _sort = Builders<TDocument>.Sort.Ascending(propertyName);
        else
            _sort = Builders<TDocument>.Sort.Descending(propertyName);
        return this;
    }

    public LoadSpecification<TDocument> OrderByProperty<TKey>(OrderDirection orderDirection, Expression<Func<TDocument, TKey>> keySelector)
    {
        var memberExpression = (MemberExpression)keySelector.Body;
        if(orderDirection == OrderDirection.ASC)
            _sort = Builders<TDocument>.Sort.Ascending(memberExpression.Member.Name);
        else
            _sort = Builders<TDocument>.Sort.Descending(memberExpression.Member.Name);
        return this;
    }

    public LoadSpecification<TDocument> OrderByPropertyNameAsc(string parameter)
    {
        _sort = Builders<TDocument>.Sort.Ascending(parameter);
        return this;
    }

    public LoadSpecification<TDocument> OrderByPropertyNameDesc(string parameter)
    {
        _sort = Builders<TDocument>.Sort.Descending(parameter);
        return this;
    }

    public LoadSpecification<TDocument> OrderByPropertyAsc<TKey>(Expression<Func<TDocument, TKey>> keySelector)
    {
        var memberExpression = (MemberExpression)keySelector.Body;
        _sort = Builders<TDocument>.Sort.Ascending(memberExpression.Member.Name);
        return this;
    }

    public LoadSpecification<TDocument> OrderByPropertyDesc<TKey>(Expression<Func<TDocument, TKey>> keySelector)
    {
        var memberExpression = (MemberExpression)keySelector.Body;
        _sort = Builders<TDocument>.Sort.Descending(memberExpression.Member.Name);
        return this;
    }
}