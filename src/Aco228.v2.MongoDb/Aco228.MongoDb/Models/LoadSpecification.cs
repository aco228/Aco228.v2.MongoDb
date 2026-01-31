using System.Linq.Expressions;
using Aco228.MongoDb.Infrastructure;
using Aco228.MongoDb.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aco228.MongoDb.Models;

public class LoadSpecification<TDocument, TProjection>
    where TDocument : MongoDocument
    where TProjection : class
{
    internal IMongoRepo<TDocument> Repo { get; set; }
    protected List<Expression<Func<TDocument, bool>>> _expressions = new();
    protected SortDefinition<TDocument>? _sort;
    private ProjectionDefinition<TDocument>? _projectionDefinition;
    internal ProjectionMapper<TProjection, TDocument>? ProjectMapper { get; set; }
    protected int? _limit;
    protected int? _skip;
    
    public LoadSpecification() { }

    public LoadSpecification(IMongoRepo<TDocument> repo)
    {
        Repo = repo;
    }

    internal LoadSpecification<TDocument, TProjection> SetRepo(IMongoRepo<TDocument> repo)
    {
        Repo = repo;
        return this;
    }
    
    public LoadSpecification<TDocument, TProjection> FilterBy(Expression<Func<TDocument, bool>>? filter)
    {
        if (filter == null) return this;
        _expressions.Add(filter);
        return this;
    }
    
    public LoadSpecification<TDocument, TProjection> Limit(int? limit)
    {
        _limit = limit;
        return this;
    }

    public LoadSpecification<TDocument, TProjection> Skip(int? skip)
    {
        _skip = skip;
        return this;
    }
    
    public LoadSpecification<TDocument, TProjection> AfterDocument(TDocument lastObject)
    {
        FilterBy(x => x.Id.CompareTo(lastObject.Id) > 0);
        return this;
    }
    public LoadSpecification<TDocument, TProjection> AfterId(ObjectId lastId)
    {
        FilterBy(x => x.Id.CompareTo(lastId) > 0);
        return this;
    }

    public LoadSpecification<TDocument, TProjection> OrderByPropertyName(OrderDirection orderDirection, string propertyName)
    {
        if(orderDirection == OrderDirection.ASC)
            _sort = Builders<TDocument>.Sort.Ascending(propertyName);
        else
            _sort = Builders<TDocument>.Sort.Descending(propertyName);
        return this;
    }

    public LoadSpecification<TDocument, TProjection> OrderByProperty<TKey>(OrderDirection orderDirection, Expression<Func<TDocument, TKey>> keySelector)
    {
        var memberExpression = keySelector.Body as MemberExpression ?? throw new ArgumentException("Expression must be a simple property access (e.g., x => x.PropertyName)", nameof(keySelector));
        if(orderDirection == OrderDirection.ASC)
            _sort = Builders<TDocument>.Sort.Ascending(memberExpression.Member.Name);
        else
            _sort = Builders<TDocument>.Sort.Descending(memberExpression.Member.Name);
        return this;
    }

    public LoadSpecification<TDocument, TProjection> OrderByPropertyNameAsc(string parameter)
    {
        _sort = Builders<TDocument>.Sort.Ascending(parameter);
        return this;
    }

    public LoadSpecification<TDocument, TProjection> OrderByPropertyNameDesc(string parameter)
    {
        _sort = Builders<TDocument>.Sort.Descending(parameter);
        return this;
    }

    public LoadSpecification<TDocument, TProjection> OrderByPropertyAsc<TKey>(Expression<Func<TDocument, TKey>> keySelector)
    {
        var memberExpression = keySelector.Body as MemberExpression ?? throw new ArgumentException("Expression must be a simple property access (e.g., x => x.PropertyName)", nameof(keySelector));
        _sort = Builders<TDocument>.Sort.Ascending(memberExpression.Member.Name);
        return this;
    }

    public LoadSpecification<TDocument, TProjection> OrderByPropertyDesc<TKey>(Expression<Func<TDocument, TKey>> keySelector)
    {
        var memberExpression = keySelector.Body as MemberExpression ?? throw new ArgumentException("Expression must be a simple property access (e.g., x => x.PropertyName)", nameof(keySelector));
        _sort = Builders<TDocument>.Sort.Descending(memberExpression.Member.Name);
        return this;
    }

    private void PrepareProjection()
    {
        if(ProjectMapper is not null && _projectionDefinition is not null)
            return;
        
        ProjectMapper = new ProjectionMapper<TProjection, TDocument>();
        _projectionDefinition = ProjectMapper.GetProjection();
    }
    
    internal FilterDefinition<TDocument> BuildFilter(Expression<Func<TDocument, bool>>? filter = null)
    {
        if (filter != null) FilterBy(filter);
        
        if (_expressions.Count == 0)
            return Builders<TDocument>.Filter.Empty;

        if (_expressions.Count == 1)
            return Builders<TDocument>.Filter.Where(_expressions[0]);

        var filters = _expressions
            .Select(expr => Builders<TDocument>.Filter.Where(expr))
            .ToList();

        return Builders<TDocument>.Filter.And(filters);
    }
    
    internal virtual IFindFluent<TDocument, TDocument> GetCursor(Expression<Func<TDocument, bool>>? filter = null)
    {
        FilterBy(filter);
        PrepareProjection();
        
        var filters = BuildFilter();
        var cursor = Repo.GetCollection().Find(filters);

        if (typeof(TDocument) != typeof(TProjection))
            cursor.Project<TDocument>(_projectionDefinition);
        
        if (_sort != null) cursor = cursor.Sort(_sort);
        if (_limit.HasValue) cursor = cursor.Limit(_limit.Value);
        if (_skip.HasValue) cursor = cursor.Skip(_skip.Value);

        return cursor;
    }
    
    internal virtual async Task<IAsyncCursor<TDocument>> GetCursorAsync(
        Expression<Func<TDocument, bool>>? filter = null,
        int? batchSize = null)
    {
        FilterBy(filter);
        PrepareProjection();
        
        var filters = BuildFilter();
        var findOptions = new FindOptions<TDocument, TDocument>();

        if (typeof(TDocument) != typeof(TProjection))
            findOptions.Projection = _projectionDefinition;
        
        if (_sort != null) findOptions.Sort = _sort;
        if (_limit.HasValue) findOptions.Limit =  _limit.Value;
        if (_skip.HasValue) findOptions.Skip = _skip.Value;
        
        var cursor = await Repo.GetCollection().FindAsync(filters,  findOptions);
        return cursor;
    }
}