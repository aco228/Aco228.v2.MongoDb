using Aco228.Common.Models;
using Aco228.MongoDb.Extensions.RepoExtensions;
using Aco228.MongoDb.Models;

namespace Aco228.MongoDb.Services;

public interface IMongoTransaction<T> : IAsyncDisposable
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
    private SemaphoreSlim _semaphore = new(1, 1);

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
        _semaphore.Wait();
        _insertRequests.Add(document);
        _semaphore.Release();
        
        TryExecute();
    }

    public void InsertOrUpdateMultiple(IEnumerable<T> documents)
    {
        _semaphore.Wait();
        _insertRequests.AddRange(documents);
        _semaphore.Release();
        
        TryExecute();
    }

    public async Task InsertOrUpdateAsync(T document)
    {
        await _semaphore.WaitAsync();
        _insertRequests.Add(document);
        _semaphore.Release();
        
        await TryExecuteAsync();
    }

    public async Task InsertOrUpdateMultipleAsync(IEnumerable<T> documents)
    {
        await _semaphore.WaitAsync();
        _insertRequests.AddRange(documents);
        _semaphore.Release();
        
        await TryExecuteAsync();
    }

    public void Delete(T document)
    {
        _semaphore.Wait();
        _deleteRequests.Add(document);
        _semaphore.Release();
        
        TryExecute();
    }

    public void DeleteMultiple(IEnumerable<T> documents)
    {
        _semaphore.Wait();
        _deleteRequests.AddRange(documents);
        _semaphore.Release();
        
        TryExecute();
    }

    public async Task DeleteAsync(T document)
    {
        await _semaphore.WaitAsync();
        _deleteRequests.Add(document);
        _semaphore.Release();
        
        await TryExecuteAsync();
    }

    public async Task DeleteMultipleAsync(IEnumerable<T> documents)
    {
        await _semaphore.WaitAsync();
        _deleteRequests.AddRange(documents);
        _semaphore.Release();
        
        await TryExecuteAsync();
    }

    private void TryExecute(bool force = false)
    {
        if(!_insertRequests.Any() && !_deleteRequests.Any()) return;
        if(!force && CurrentCount < _limit) return;

        _semaphore.Wait();
        try
        {
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
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task TryExecuteAsync(bool force = false)
    {
        if(!_insertRequests.Any() && !_deleteRequests.Any()) return;
        if(!force && CurrentCount < _limit) return;

        await _semaphore.WaitAsync();
        try
        {
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
        finally
        {
            _semaphore.Release();
        }
    }

    public void Finish()
        => TryExecute(true);

    public async Task FinishAsync()
    {
        await TryExecuteAsync(true);
    }

    public async Task FinishAndDisposeAsync()
    {
        await FinishAsync();
        await DisposeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_semaphore is IAsyncDisposable semaphoreAsyncDisposable)
            await semaphoreAsyncDisposable.DisposeAsync();
        else
            _semaphore.Dispose();
    }
}