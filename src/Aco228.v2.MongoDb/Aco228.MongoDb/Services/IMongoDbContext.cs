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
    private MongoClient? _client;
    public abstract string DatabaseName { get; }
    
    public IMongoDatabase GetDatabase()
    {
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
            new IgnoreIfNullConvention(true)
        };

        ConventionRegistry.Register("Ignore null values globally", pack, t => true);
        return _client.GetDatabase(DatabaseName);
    }
    
    
    protected abstract string GetConnectionString();
    protected virtual MongoClientSettings ConfigureClientSettings(MongoClientSettings settings) => settings;

    public void Dispose()
    {
        _client?.Dispose();
    }
}