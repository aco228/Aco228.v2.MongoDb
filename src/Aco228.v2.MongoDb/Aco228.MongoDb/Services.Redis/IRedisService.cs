using Newtonsoft.Json;
using StackExchange.Redis;

namespace Aco228.MongoDb.Services.Redis;

public interface IRedisService
{
    Task SetStringAsync(string key, string value, TimeSpan? expiry = null);
    Task<string?> GetStringAsync(string key);
    
    Task SetIntAsync(string key, int value, TimeSpan? expiry = null);
    Task<int?> GetIntAsync(string key);

    Task SetObjectAsync<T>(string key, T obj, TimeSpan? expiry = null);
    Task<T?> GetObjectAsync<T>(string key);

    Task<bool> ExpireAsync(string key, TimeSpan ttl);
    Task<TimeSpan?> GetTtlAsync(string key);
    Task<bool> DeleteAsync(string key);
    Task<bool> ExistsAsync(string key);

    Task<bool> TryAcquireLockAsync(string resource, TimeSpan expiry);
    Task<bool> ReleaseLockAsync(string resource, string token);
}

public abstract class RedisService : IRedisService, IAsyncDisposable
{
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    protected virtual string NamePrefix { get; } = "";

    private string GetKeyName(string key)
    {
        if (string.IsNullOrEmpty(NamePrefix))
            return key;
        return $"{NamePrefix}-{key}";
    }

    public RedisService(string envVariableName)
    {
        var connectionString = Environment.GetEnvironmentVariable(envVariableName);
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentException("Please provide connection string for Redis service");
        
        _redis = ConnectionMultiplexer.Connect(connectionString);
        _db = _redis.GetDatabase();
    }

    public async Task SetStringAsync(string key, string value, TimeSpan? expiry = null)
    {
        if (expiry.HasValue)
            await _db.StringSetAsync(GetKeyName(key), value, expiry.Value); // TimeSpan, not TimeSpan?
        else
            await _db.StringSetAsync(GetKeyName(key), value);
    }

    public async Task<string?> GetStringAsync(string key)
    {
        var value = await _db.StringGetAsync(GetKeyName(key));
        return value.HasValue ? value.ToString() : null;
    }

    public async Task SetIntAsync(string key, int value, TimeSpan? expiry = null)
    {
        if (expiry.HasValue)
            await _db.StringSetAsync(GetKeyName(key), value.ToString(), expiry.Value); // TimeSpan, not TimeSpan?
        else
            await _db.StringSetAsync(GetKeyName(key), value.ToString());
    }

    public async Task<int?> GetIntAsync(string key)
    {
        var value = await _db.StringGetAsync(GetKeyName(key));
        return value.HasValue && int.TryParse(value.ToString(), out var res) ? res : null;
    }

    // --- Store an object as JSON ---
    public async Task SetObjectAsync<T>(string key, T obj, TimeSpan? expiry = null)
    {
        var json = JsonConvert.SerializeObject(obj);
        if (expiry.HasValue)
            await _db.StringSetAsync(GetKeyName(key), json, expiry.Value);
        else
            await _db.StringSetAsync(GetKeyName(key), json);
    }

    public async Task<T?> GetObjectAsync<T>(string key)
    {
        var json = await _db.StringGetAsync(GetKeyName(key));
        if (!json.HasValue) return default;
        return JsonConvert.DeserializeObject<T>(json.ToString());
    }
    
    // ---------------------------------------------------------------------
    // TTL / KEY MANAGEMENT
    // ---------------------------------------------------------------------
 
    public Task<bool> ExpireAsync(string key, TimeSpan ttl)
        => _db.KeyExpireAsync(GetKeyName(key), ttl);
 
    public Task<TimeSpan?> GetTtlAsync(string key)
        => _db.KeyTimeToLiveAsync(GetKeyName(key));
 
    public Task<bool> DeleteAsync(string key)
        => _db.KeyDeleteAsync(GetKeyName(key));
 
    public Task<bool> ExistsAsync(string key)
        => _db.KeyExistsAsync(GetKeyName(key));
    
    /// <summary>
    /// Attempts to acquire a lock. Returns a token if successful (needed to safely
    /// release it) or null if someone else already holds it.
    /// </summary>
    public async Task<bool> TryAcquireLockAsync(string resource, TimeSpan expiry)
    {
        var token = Guid.NewGuid().ToString();
        var key = $"lock:{resource}";
        var acquired = await _db.StringSetAsync(GetKeyName(key), token, expiry, When.NotExists);
        return acquired;
    }
 
    /// <summary>
    /// Releases the lock only if the caller still holds it (token matches) —
    /// prevents accidentally releasing a lock someone else acquired after yours expired.
    /// </summary>
    public async Task<bool> ReleaseLockAsync(string resource, string token)
    {
        const string script = @"
            if redis.call('get', KEYS[1]) == ARGV[1] then
                return redis.call('del', KEYS[1])
            else
                return 0
            end";
        var key = $"lock:{resource}";
        var result = await _db.ScriptEvaluateAsync(script, new RedisKey[] { GetKeyName(key) }, new RedisValue[] { token });
        return (long)result == 1;
    }

    public async ValueTask DisposeAsync()
    {
        await _redis.DisposeAsync();
    }
}