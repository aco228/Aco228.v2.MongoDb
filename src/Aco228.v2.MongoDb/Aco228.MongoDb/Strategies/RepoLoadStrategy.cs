using System.Linq.Expressions;
using Aco228.MongoDb.Infrastructure;
using Aco228.MongoDb.Models;
using Aco228.MongoDb.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aco228.MongoDb.Strategies;

public class RepoLoadStrategy<TDocument, TProjection>
    where TDocument : MongoDocument
    where TProjection : class
{
    private readonly IMongoRepo<TDocument> _repo;
    
    private ProjectionDefinition<TDocument>? _projectionDefinition;
    private List<Expression<Func<TDocument, bool>>> _expressions = new();
    private ProjectionMapper<TProjection, TDocument>? _projectionMapper;

    private SortDefinition<TDocument>? _sort;
    private int? _limit;
    private int? _skip;

    internal RepoLoadStrategy(IMongoRepo<TDocument> repo)
    {
        _repo = repo;

        if (typeof(TDocument) != typeof(TProjection))
        {
            _projectionMapper = new ProjectionMapper<TProjection, TDocument>();
            _projectionDefinition = _projectionMapper.GetProjection();
        }
        
    }

    public RepoLoadStrategy<TDocument, TProjection> Limit(int? limit)
    {
        _limit = limit;
        return this;
    }

    public RepoLoadStrategy<TDocument, TProjection> Skip(int? skip)
    {
        _skip = skip;
        return this;
    }

    public RepoLoadStrategy<TDocument, TProjection> AfterDocument(TDocument lastObject)
    {
        FilterBy(x => x.Id.CompareTo(lastObject.Id) > 0);
        return this;
    }
    public RepoLoadStrategy<TDocument, TProjection> AfterId(ObjectId lastId)
    {
        FilterBy(x => x.Id.CompareTo(lastId) > 0);
        return this;
    }

    public RepoLoadStrategy<TDocument, TProjection> OrderByPropertyName(OrderDirection orderDirection, string propertyName)
    {
        if(orderDirection == OrderDirection.ASC)
            _sort = Builders<TDocument>.Sort.Ascending(propertyName);
        else
            _sort = Builders<TDocument>.Sort.Descending(propertyName);
        return this;
    }

    public RepoLoadStrategy<TDocument, TProjection> OrderByProperty<TKey>(OrderDirection orderDirection, Expression<Func<TDocument, TKey>> keySelector)
    {
        var memberExpression = (MemberExpression)keySelector.Body;
        if(orderDirection == OrderDirection.ASC)
            _sort = Builders<TDocument>.Sort.Ascending(memberExpression.Member.Name);
        else
            _sort = Builders<TDocument>.Sort.Descending(memberExpression.Member.Name);
        return this;
    }

    public RepoLoadStrategy<TDocument, TProjection> OrderByPropertyNameAsc(string parameter)
    {
        _sort = Builders<TDocument>.Sort.Ascending(parameter);
        return this;
    }

    public RepoLoadStrategy<TDocument, TProjection> OrderByPropertyNameDesc(string parameter)
    {
        _sort = Builders<TDocument>.Sort.Descending(parameter);
        return this;
    }

    public RepoLoadStrategy<TDocument, TProjection> OrderByPropertyAsc<TKey>(Expression<Func<TDocument, TKey>> keySelector)
    {
        var memberExpression = (MemberExpression)keySelector.Body;
        _sort = Builders<TDocument>.Sort.Ascending(memberExpression.Member.Name);
        return this;
    }

    public RepoLoadStrategy<TDocument, TProjection> OrderByPropertyDesc<TKey>(Expression<Func<TDocument, TKey>> keySelector)
    {
        var memberExpression = (MemberExpression)keySelector.Body;
        _sort = Builders<TDocument>.Sort.Descending(memberExpression.Member.Name);
        return this;
    }

    public RepoLoadStrategy<TDocument, TProjection> FilterBy(Expression<Func<TDocument, bool>>? filter)
    {
        if (filter == null) return this;
        _expressions.Add(filter);
        return this;
    }

    #region  # Collections

    public IEnumerable<TProjection> ToEnumerable(Expression<Func<TDocument, bool>>? filter = null)
    {
        if (filter != null) FilterBy(filter);

        var filters = BuildFilter();
        var cursor = _repo.GetCollection().Find(filters);
        cursor = AppendCursor(cursor);

        if (typeof(TDocument) == typeof(TProjection))
            return cursor.ToEnumerable() as IEnumerable<TProjection>;

        if (_projectionDefinition == null || _projectionMapper == null)
            throw new InvalidOperationException($"ProjectionDefinition and ProjectionMapper must be set.");

        cursor = cursor.Project<TDocument>(_projectionDefinition);
        return _projectionMapper.CreateObjectsFrom(cursor.ToEnumerable());
    }
    
    public List<TProjection> ToList(Expression<Func<TDocument, bool>>? filter = null)
    {
        if (filter != null) FilterBy(filter);

        var filters = BuildFilter();
        var cursor = _repo.GetCollection().Find(filters);
        cursor = AppendCursor(cursor);

        if (typeof(TDocument) == typeof(TProjection))
            return cursor.ToList() as List<TProjection>;

        if (_projectionDefinition == null || _projectionMapper == null)
            throw new InvalidOperationException($"ProjectionDefinition and ProjectionMapper must be set.");

        cursor = cursor.Project<TDocument>(_projectionDefinition);
        return _projectionMapper.CreateObjectsFrom(cursor.ToEnumerable()).ToList();
    }

    public async Task<List<TProjection>> ToListAsync(Expression<Func<TDocument, bool>>? filter = null)
    {
        if (filter != null) FilterBy(filter);

        var filters = BuildFilter();
        var cursor = _repo.GetCollection().Find(filters);
        cursor = AppendCursor(cursor);

        if (typeof(TDocument) == typeof(TProjection))
            return (await cursor.ToListAsync()) as List<TProjection>;

        if (_projectionDefinition == null || _projectionMapper == null)
            throw new InvalidOperationException($"ProjectionDefinition and ProjectionMapper must be set.");

        var documents = await cursor.Project<TDocument>(_projectionDefinition).ToListAsync();
        return _projectionMapper.CreateObjectsFrom(documents).ToList();
    }

    #endregion

    #region # Single

    public TProjection FirstOrDefault(Expression<Func<TDocument, bool>>? filter = null)
    {
        if (filter != null) FilterBy(filter);

        var filters = BuildFilter();
        var cursor = _repo.GetCollection().Find(filters);
        cursor = AppendCursor(cursor);

        if (typeof(TDocument) == typeof(TProjection))
            return cursor.FirstOrDefault() as TProjection;

        if (_projectionDefinition == null || _projectionMapper == null)
            throw new InvalidOperationException($"ProjectionDefinition and ProjectionMapper must be set.");

        cursor = cursor.Project<TDocument>(_projectionDefinition);
        return _projectionMapper.CreateObjectFrom(cursor.FirstOrDefault());
    }
    

    public async Task<TProjection> FirstOrDefaultAsync(Expression<Func<TDocument, bool>>? filter = null)
    {
        if (filter != null) FilterBy(filter);

        var filters = BuildFilter();
        var cursor = _repo.GetCollection().Find(filters);
        cursor = AppendCursor(cursor);
        
        if (typeof(TDocument) == typeof(TProjection))
            return await cursor.FirstOrDefaultAsync() as TProjection;

        if (_projectionDefinition == null || _projectionMapper == null)
            throw new InvalidOperationException($"ProjectionDefinition and ProjectionMapper must be set.");

        cursor = cursor.Project<TDocument>(_projectionDefinition);
        var document = await cursor.FirstOrDefaultAsync();
        return _projectionMapper.CreateObjectFrom(document);
    }

    #endregion

    #region Count

    public Task<long> CountAsync(Expression<Func<TDocument, bool>>? filter = null)
    {
        var filters = BuildFilter(filter);
        return _repo.GetCollection().CountDocumentsAsync(filters);
    }
    
    public long Count(Expression<Func<TDocument, bool>>? filter = null)
    {
        var filters = BuildFilter(filter);
        return _repo.GetCollection().CountDocuments(filters);
    }

    #endregion

    private FilterDefinition<TDocument> BuildFilter(Expression<Func<TDocument, bool>>? filter = null)
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

    private IFindFluent<TDocument, TDocument> AppendCursor(IFindFluent<TDocument, TDocument> cursor)
    {
        if (_sort != null) cursor = cursor.Sort(_sort);
        if (_limit.HasValue) cursor = cursor.Limit(_limit.Value);
        if (_skip.HasValue) cursor = cursor.Skip(_skip.Value);
        return cursor;
    }
}