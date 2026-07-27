using Aco228.Common;
using Aco228.Common.Services;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Aco228.MongoDb.Services.Redis;

public interface IRedisService
{
    ConnectionMultiplexer Multiplexer { get; }
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
    Task<bool> DeleteLockAsync(string resource);
}

public abstract class RedisService : IRedisService, IAsyncDisposable
{
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    protected virtual string NamePrefix { get; } = "";

    public ConnectionMultiplexer Multiplexer => _redis;

    private string GetKeyName(string key)
    {
        if (string.IsNullOrEmpty(NamePrefix))
            return key;
        return $"{NamePrefix}-{key}";
    }

    public RedisService(string? envVariableName = null)
    {
        if (string.IsNullOrEmpty(envVariableName))
            envVariableName = "REDIS_CONNECTION_STRING";
        
        var connectionString = Environment.GetEnvironmentVariable(envVariableName);
        if (string.IsNullOrEmpty(connectionString))
        {
            var secretProvider = ServiceProviderHelper.GetService<ISecretProvider>()!;
            connectionString = secretProvider.Get(envVariableName);
        }
        
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

    public async Task<bool> DeleteLockAsync(string resource)
    {
        var key = $"lock:{resource}";
        var res = await _db.KeyDeleteAsync(GetKeyName(key));
        return res;
    }

    public async ValueTask DisposeAsync()
    {
        await _redis.DisposeAsync();
    }
}