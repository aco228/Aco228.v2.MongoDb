using System.Reflection;
using Aco228.Common;
using Aco228.Common.Extensions;
using Aco228.MongoDb.Extensions.RepoExtensions;
using Aco228.MongoDb.Models;
using Aco228.MongoDb.Models.Attributes;
using Aco228.MongoDb.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aco228.MongoDb.Helpers;

public static class MongoRepoHelpers
{
    public static IMongoRepo<TDocument> CreateRepo<TDocument, TDbContext>()
        where TDbContext : IMongoDbContext
        where TDocument : MongoDocument
    {
        var documentType = typeof(TDocument);
        if(documentType.IsAbstract)
            return null;
        
        var configurationAttribute = documentType.GetCustomAttribute<BsonCollectionAttribute>();
        if(configurationAttribute == null)
            throw new ArgumentException($"{documentType.Name} doesn't have BsonCollectionAttribute");

        var mongoDbContextType = typeof(TDbContext);
        var mongoDbContext = ServiceProviderHelper.GetServiceByType(mongoDbContextType) as IMongoDbContext;
        if(mongoDbContext == null)
            throw new ArgumentException($"{documentType.Name} doesn't have proper IMongoDbContext");
        
        var repoService = Activator.CreateInstance<MongoRepo<TDocument>>() as MongoRepo<TDocument>;
        repoService.Configure(configurationAttribute, mongoDbContext);
        return repoService;
    }
    
    private static void RegisterRepository<TDocument, TDbContext>(IServiceCollection services)
        where TDbContext : IMongoDbContext
        where TDocument : MongoDocument
    {
        services.AddTransient<IMongoRepo<TDocument>>(provider =>
        {
            var repo = CreateRepo<TDocument, TDbContext>();
            return repo;
        });
    }

    public static void RegisterRepositoriesFromAssembly<TDbContext>(this IServiceCollection services)
        where TDbContext : IMongoDbContext
    {
        var assembly = Assembly.GetAssembly(typeof(TDbContext));
        var assemblyTypes = assembly.GetTypes();
        
        if(!typeof(TDbContext).IsInterface)
            throw new InvalidOperationException("IMongoDbContext interface not found");
        
        var registerRepositoryMethod = typeof(MongoRepoHelpers).GetMethod(nameof(RegisterRepository), BindingFlags.NonPublic | BindingFlags.Static);
        if (registerRepositoryMethod == null)
            throw new InvalidOperationException("Can't find RegisterRepository method");
        
        foreach (var assemblyType in assemblyTypes)
        {
            if (!typeof(MongoDocument).IsAssignableFrom(assemblyType) || assemblyType.IsAbstract)
                continue;
            
            var registerRepositoryGeneric = registerRepositoryMethod.MakeGenericMethod(assemblyType, typeof(TDbContext));
            registerRepositoryGeneric.Invoke(null, new object[] { services });
        }

        // services.RegisterPostBuildActionAsync(async (pr) =>
        // {
        //     await ConfigureMongoIndexesFromAssembly<TDbContext>(assembly); 
        // });
    }

    private static async Task ConfigureMongoIndexesFromAssembly<TDbContext>(Assembly assembly)
        where TDbContext : IMongoDbContext
    {
        var assemblyTypes = assembly.GetTypes();
        foreach (var assemblyType in assemblyTypes)
        {
            if (!typeof(MongoDocument).IsAssignableFrom(assemblyType) || assemblyType.IsAbstract)
                continue;
            
            var repoType = typeof(IMongoRepo<>).MakeGenericType(assemblyType);
            var repoService = ServiceProviderHelper.GetServiceByType(repoType);
            if (repoService == null)
                continue;
            
            // MongoIndexesExtensions.ConfigureIndexesAsync()
            

            int a = 0;

            // var repo = CreateRepo<TDocument, TDbContext>();
            // await repo.ConfigureIndexesAsync();

            // var method = typeof(MongoRepoHelpers).GetMethod(nameof(CreateRepo), BindingFlags.Public | BindingFlags.Static);
            // var genericMethod = method?.MakeGenericMethod(assemblyType);
            // var service = genericMethod?.Invoke(null, null);
            // if (service == null)
            //     continue;
            //
            // var configureIndexesAsyncMethod = typeof(MongoIndexesExtensions).GetMethod( nameof(MongoIndexesExtensions.ConfigureIndexesAsync), BindingFlags.Public | BindingFlags.Static);
            // var configureIndexesAsyncMethodGeneric = configureIndexesAsyncMethod?.MakeGenericMethod(assemblyType);
            // configureIndexesAsyncMethodGeneric?.Invoke(null, new []{ service });
        }
    }
    
}