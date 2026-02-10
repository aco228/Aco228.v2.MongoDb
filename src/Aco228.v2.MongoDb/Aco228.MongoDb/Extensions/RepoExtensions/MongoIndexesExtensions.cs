using System.Reflection;
using Aco228.Common.Extensions;
using Aco228.MongoDb.Models.Attributes;
using Aco228.MongoDb.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aco228.MongoDb.Extensions.RepoExtensions;

public static class MongoIndexesExtensions
{
    private record MongoIndex
    {
        public string MongoName { get; set; }
        public string Name { get; set; }
    }

    public static async Task ConfigureIndexes(IMongoDbContext dbContext, Type documentType)
    {
        var attribute = documentType.GetCustomAttribute<BsonCollectionAttribute>();
        if (attribute == null)
            return;
        
        if(documentType.IsInterface || documentType.IsAbstract)
            return;
        
        
        Console.WriteLine($"Configuring {documentType.Name}.{attribute.CollectionName}");
        var indexProps = documentType.GetPropertyWithAttribute<MongoIndexAttribute>();
        var collection = dbContext.GetDatabase().GetCollection<BsonDocument>(attribute.CollectionName);
        if(collection == null)
            return;
        
        var currentIndexes = new List<MongoIndex>();
        
        var indexes = (await collection.Indexes.ListAsync()) as IAsyncCursor<BsonDocument>;
        var indexList = await indexes.ToListAsync();
        
        foreach (var indexName in indexList.Select(x => x["name"].ToString()))
        {
            if (indexName == "_id_")
                continue;
                
            currentIndexes.Add(new()
            {
                Name = indexName.Split("_").First(),
                MongoName = indexName, 
            });
        }
        
        // Create new indexes
        foreach (var (indexProperty, indexAttribute) in indexProps)
        {
            if (currentIndexes.Any(x => x.Name == indexProperty.Name))
                continue;

            Console.WriteLine($"Creating index {documentType.Name}.{indexProperty.Name}");
            await collection.CreateIndexAsync(indexProperty.Name, indexAttribute.IsUnique);
        }

        // Delete old indexes
        foreach (var indexName in currentIndexes)
        {
            if (indexProps.Any(x => x.Info.Name == indexName.Name))
                continue;
            
            Console.WriteLine($"Deleting index {documentType.Name}.{indexName.MongoName}");
            await collection.DeleteIndexAsync(indexName.MongoName);
        }
    }
    
    public static Task CreateIndexAsync(this IMongoCollection<BsonDocument> collection, string fieldName, bool isUnique = false)
    {
        var options = new CreateIndexOptions { Unique = isUnique };
        var indexDefinition = Builders<BsonDocument>.IndexKeys.Ascending(fieldName);
        return collection.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(indexDefinition, options));
    }
    
    public static Task DeleteIndexAsync(this IMongoCollection<BsonDocument> collection, string indexName)
    {
        return collection.Indexes.DropOneAsync(indexName);
    }
}