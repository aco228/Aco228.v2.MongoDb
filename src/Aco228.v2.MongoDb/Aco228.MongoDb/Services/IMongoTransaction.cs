using Aco228.Common.Models;
using Aco228.MongoDb.Extensions.RepoExtensions;
using Aco228.MongoDb.Models;

namespace Aco228.MongoDb.Services;

public interface IMongoTransaction<T>
    where T : MongoDocument

{
    MongoTransaction<T> SetLimit(int limit);
    
    void InsertOrUpdate(T document);
    void InsertOrUpdateMultiple(IEnumerable<T> documents);
    Task InsertOrUpdateAsync(T document);
    Task InsertOrUpdateMultipleAsync(IEnumerable<T> documents);
    
    
    void Delete(T document);
    void DeleteMultiple(IEnumerable<T> documents);
    Task DeleteAsync(T document);
    Task DeleteMultipleAsync(IEnumerable<T> documents);

    void Finish();
    Task FinishAsync();
}

public class MongoTransaction<T> : IMongoTransaction<T>
    where T : MongoDocument
{
    private ConcurrentList<T> _insertRequests = new();
    private ConcurrentList<T> _deleteRequests = new();
    public IMongoRepo<T> Repo { get; private set; }
    private int CurrentCount => _insertRequests.Count + _deleteRequests.Count;
    private int _limit = 15;

    public MongoTransaction(IMongoRepo<T> repo)
    {
        Repo = repo;
    }

    public MongoTransaction<T> SetLimit(int limit)
    {
        _limit =  limit;
        return this;
    }

    public void InsertOrUpdate(T document)
    {
        _insertRequests.Add(document);
        TryExecute();
    }

    public void InsertOrUpdateMultiple(IEnumerable<T> documents)
    {
        _insertRequests.AddRange(documents);
        TryExecute();
    }

    public Task InsertOrUpdateAsync(T document)
    {
        _insertRequests.Add(document);
        return TryExecuteAsync();
    }

    public Task InsertOrUpdateMultipleAsync(IEnumerable<T> documents)
    {
        _insertRequests.AddRange(documents);
        return TryExecuteAsync();
    }

    public void Delete(T document)
    {
        _deleteRequests.Add(document);
        TryExecute();
    }

    public void DeleteMultiple(IEnumerable<T> documents)
    {
        _deleteRequests.AddRange(documents);
        TryExecute();
    }

    public Task DeleteAsync(T document)
    {
        _deleteRequests.Add(document);
        return TryExecuteAsync();
    }

    public Task DeleteMultipleAsync(IEnumerable<T> documents)
    {
        _deleteRequests.AddRange(documents);
        return TryExecuteAsync();
    }

    private void TryExecute(bool force = false)
    {
        if(!_insertRequests.Any() && !_deleteRequests.Any()) return;
        if(!force && CurrentCount < _limit) return;

        if (_insertRequests.Any())
        {
            Repo.InsertOrUpdateMany(_insertRequests);
            _insertRequests.Clear();
        }

        if (_deleteRequests.Any())
        {
            Repo.DeleteMany(_deleteRequests);
            _deleteRequests.Clear();
        }
    }

    private async Task TryExecuteAsync(bool force = false)
    {
        if(!_insertRequests.Any() && !_deleteRequests.Any()) return;
        if(!force && CurrentCount < _limit) return;

        if (_insertRequests.Any())
        {
            await Repo.InsertOrUpdateManyAsync(_insertRequests);
            _insertRequests.Clear();
        }

        if (_deleteRequests.Any())
        {
            await Repo.DeleteManyAsync(_deleteRequests);
            _deleteRequests.Clear();
        }
    }

    public void Finish()
        => TryExecute(true);

    public Task FinishAsync()
        => TryExecuteAsync(true);
}