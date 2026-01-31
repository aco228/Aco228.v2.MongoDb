using Aco228.MongoDb.Extensions.RepoExtensions;
using Aco228.MongoDb.Models;

namespace Aco228.MongoDb.Services;

public interface IMongoRepoTransactionalManager<T>
    where T : MongoDocument

{
    MongoRepoTransactionalManager<T> SetLimit(int limit);
    
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

public class MongoRepoTransactionalManager<T> : IMongoRepoTransactionalManager<T>
    where T : MongoDocument
{
    private List<T> _insertRequests = new();
    private List<T> _deleteRequests = new();
    private readonly IMongoRepo<T> _repo;
    private int CurrentCount => _insertRequests.Count + _deleteRequests.Count;
    private int _limit = 15;

    public MongoRepoTransactionalManager(IMongoRepo<T> repo)
    {
        _repo = repo;
    }

    public MongoRepoTransactionalManager<T> SetLimit(int limit)
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
            _repo.InsertOrUpdateMany(_insertRequests);
            _insertRequests.Clear();
        }

        if (_deleteRequests.Any())
        {
            _repo.DeleteMany(_deleteRequests);
            _deleteRequests.Clear();
        }
    }

    private async Task TryExecuteAsync(bool force = false)
    {
        if(!_insertRequests.Any() && !_deleteRequests.Any()) return;
        if(!force && CurrentCount < _limit) return;

        if (_insertRequests.Any())
        {
            await _repo.InsertOrUpdateManyAsync(_insertRequests);
            _insertRequests.Clear();
        }

        if (_deleteRequests.Any())
        {
            await _repo.DeleteManyAsync(_deleteRequests);
            _deleteRequests.Clear();
        }
    }

    public void Finish()
        => TryExecute(true);

    public Task FinishAsync()
        => TryExecuteAsync(true);
}