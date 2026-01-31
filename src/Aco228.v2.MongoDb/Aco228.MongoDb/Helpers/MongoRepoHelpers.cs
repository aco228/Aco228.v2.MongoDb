using System.Reflection;
using Aco228.Common;
using Aco228.MongoDb.Models;
using Aco228.MongoDb.Models.Attributes;
using Aco228.MongoDb.Services;

namespace Aco228.MongoDb.Helpers;

public static class MongoRepoHelpers
{
    public static IMongoRepo<TDocument> CreateRepo<TDocument>()
        where TDocument : MongoDocument
    {
        var configurationAttribute = typeof(TDocument).GetCustomAttribute<BsonCollectionAttribute>();
        if(configurationAttribute == null)
            throw new ArgumentException($"{typeof(TDocument).Name} doesn't have BsonCollectionAttribute");

        var mongoDbContext = ServiceProviderHelper.GetServiceByType(configurationAttribute.DbContextType) as IMongoDbContext;
        if(mongoDbContext == null)
            throw new ArgumentException($"{typeof(TDocument).Name} doesn't have proper IMongoDbContext");
        
        var repoService = Activator.CreateInstance<MongoRepo<TDocument>>() as MongoRepo<TDocument>;
        repoService.Configure(configurationAttribute, mongoDbContext);
        return repoService;
    }
    
}