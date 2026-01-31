using Aco228.Common.Models;
using Aco228.MongoDb.Services;

namespace Aco228.MongoDb.Consoler.Database;

public interface IArbDbContext : IMongoDbContext, ISingleton
{
    
}

public class ArbDbContext : MongoDbContext, IArbDbContext 
{
    public override string DatabaseName => "ARB";
   
    
    protected override string GetConnectionString()
    {
        var connectionString = Environment.GetEnvironmentVariable("ARB_MONGO_CONNECTION");
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentException($"Connection string missing");
        return connectionString;
    }
}