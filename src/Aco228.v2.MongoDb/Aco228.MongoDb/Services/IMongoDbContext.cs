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
                settings.SocketTimeout = TimeSpan.FromMinutes(5); // Adjust as needed
                settings.ConnectTimeout = TimeSpan.FromSeconds(10); // Adjust as needed
                settings.MaxConnectionIdleTime = TimeSpan.FromMinutes(5);
                settings.MaxConnectionLifeTime = TimeSpan.FromMinutes(10);

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

    protected string GetConnectionStringFromEnv(string envName)
        => Environment.GetEnvironmentVariable(envName) ?? throw new Exception($"Environment variable {envName} is not set");

    public void Dispose()
    {
        _client?.Dispose();
    }
}