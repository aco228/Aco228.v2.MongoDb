using System.Reflection;
using Aco228.Common;
using Aco228.MongoDb.Extensions.RepoExtensions;
using Aco228.MongoDb.Models;
using Aco228.MongoDb.Models.Attributes;
using Aco228.MongoDb.Services;
using Microsoft.Extensions.DependencyInjection;

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
    
    private static void RegisterRepository<TDocument>(IServiceCollection services)
        where TDocument : MongoDocument
    {
        services.AddTransient<IMongoRepo<TDocument>>(provider => CreateRepo<TDocument>());
    }

    public static void RegisterRepositoriesFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var assemblyTypes = assembly.GetTypes();
        foreach (var assemblyType in assemblyTypes)
        {
            if (!typeof(MongoDocument).IsAssignableFrom(assemblyType) || assemblyType.IsAbstract)
                continue;
            
            var method = typeof(MongoRepoHelpers).GetMethod(nameof(RegisterRepository), BindingFlags.NonPublic | BindingFlags.Static);
            var genericMethod = method.MakeGenericMethod(assemblyType);
            genericMethod.Invoke(null, new object[] { services });
        }
    }

    public static async Task ConfigureMongoIndexesFromAssembly(this Assembly assembly)
    {
        var assemblyTypes = assembly.GetTypes();
        foreach (var assemblyType in assemblyTypes)
        {
            if (!typeof(MongoDocument).IsAssignableFrom(assemblyType) || assemblyType.IsAbstract)
                continue;
            
            var method = typeof(MongoRepoHelpers).GetMethod(nameof(CreateRepo), BindingFlags.Public | BindingFlags.Static);
            var genericMethod = method?.MakeGenericMethod(assemblyType);
            var service = genericMethod?.Invoke(null, null);
            if (service == null)
                continue;

            var configureIndexesAsyncMethod = typeof(MongoIndexesExtensions).GetMethod( nameof(MongoIndexesExtensions.ConfigureIndexesAsync), BindingFlags.Public | BindingFlags.Static);
            var configureIndexesAsyncMethodGeneric = configureIndexesAsyncMethod?.MakeGenericMethod(assemblyType);
            configureIndexesAsyncMethodGeneric?.Invoke(null, new []{ service });
            
        }
    }
    
}