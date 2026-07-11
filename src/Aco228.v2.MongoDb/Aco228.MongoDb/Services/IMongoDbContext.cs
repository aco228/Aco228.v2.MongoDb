using Aco228.Common;
using Aco228.Common.Services;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Aco228.MongoDb.Services;

public interface IMongoDbContext : IDisposable
{
    string DatabaseName { get; }
    IMongoDatabase GetDatabase();
}

public abstract class MongoDbContext : IMongoDbContext
{
    private object _lock = new();
    private MongoClient? _client;
    private IMongoDatabase? _database;
    public abstract string DatabaseName { get; }
    protected virtual string ConnectionStringSecretName { get; } = "DATABASE_CONNECTION_STRING";
    private static string? _sharedConnectionString;
    
    public IMongoDatabase GetDatabase()
    {
        lock (_lock)
        {
            if (_database != null)
                return _database;
        
            if (_client == null)
            {
                MongoClientSettings settings = MongoClientSettings.FromConnectionString(GetConnectionString());
                settings.RetryWrites = true;
                settings.RetryReads = true;
                settings.ServerApi = new ServerApi(ServerApiVersion.V1);
                settings.SocketTimeout = TimeSpan.FromSeconds(60); // Adjust as needed
                settings.ConnectTimeout = TimeSpan.FromSeconds(10); // Adjust as needed
                settings.MaxConnectionIdleTime = TimeSpan.FromSeconds(60);
                settings.MaxConnectionLifeTime = TimeSpan.FromSeconds(60);
                settings.HeartbeatInterval = TimeSpan.FromSeconds(10);
                

                settings = ConfigureClientSettings(settings);
                _client = new MongoClient(settings);
            }
        
            var pack = new ConventionPack
            {
                new IgnoreIfNullConvention(true),
                new IgnoreExtraElementsConvention(true),
            };

            ConventionRegistry.Register("_", pack, t => true);
            _database = _client.GetDatabase(DatabaseName);
            return _database;
        }
    }
    
    protected abstract string GetConnectionString();
    protected virtual MongoClientSettings ConfigureClientSettings(MongoClientSettings settings) => settings;

    protected string GetConnectionStringFromEnv(string? localEnvName = null)
    {
        if (!string.IsNullOrEmpty(localEnvName))
        {
            var localConnectionString = Environment.GetEnvironmentVariable(localEnvName);
            if (localConnectionString != null)
                return localConnectionString;
        }

        var localOverride = Environment.GetEnvironmentVariable($"LOCAL_{ConnectionStringSecretName}");
        if (!string.IsNullOrEmpty(localOverride))
        {
            _sharedConnectionString = localOverride;
            return _sharedConnectionString;
        }
        

        var mainConnectionString = GetSharedConnectionString();
        return mainConnectionString ?? throw new Exception($"Environment variable {ConnectionStringSecretName} is not set");
    }

    private string GetSharedConnectionString()
    {
        if (!string.IsNullOrEmpty(_sharedConnectionString))
            return _sharedConnectionString;
        
        var mainConnectionString = ServiceProviderHelper.GetService<ISecretProvider>()!.Get(ConnectionStringSecretName);
        if(!string.IsNullOrEmpty(mainConnectionString))
            _sharedConnectionString = mainConnectionString;

        return mainConnectionString;
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}