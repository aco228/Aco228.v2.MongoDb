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
        public long? ExpireAfterSeconds { get; set; }
    }

    public static async Task ConfigureIndexes(IMongoDbContext dbContext, Type documentType)
    {
        var attribute = documentType.GetCustomAttribute<BsonCollectionAttribute>();
        if (attribute == null)
            return;
        
        if(documentType.IsInterface || documentType.IsAbstract)
            return;
        
        
        Console.WriteLine($"Configuring {documentType.Name}.{attribute.CollectionName}");
        var ttlProps = documentType.GetPropertyWithAttribute<MongoTtlAttribute>();
        foreach (var (ttlProperty, ttlAttribute) in ttlProps)
        {
            if (ttlProperty.PropertyType != typeof(DateTime) && ttlProperty.PropertyType != typeof(DateTime?))
                throw new InvalidOperationException($"[MongoTtl] on {documentType.Name}.{ttlProperty.Name} must be DateTime, Mongo ignores TTL on other types");
            if (ttlAttribute!.ExpireAfterSeconds <= 0)
                throw new InvalidOperationException($"[MongoTtl] on {documentType.Name}.{ttlProperty.Name} needs a positive expiry");
        }

        // TTL index replaces the plain index on the same field, Mongo allows only one index per key
        var indexProps = documentType.GetPropertyWithAttribute<MongoIndexAttribute>()
            .Where(x => ttlProps.All(t => t.Info.Name != x.Info.Name))
            .ToList();
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

        foreach (var index in indexList)
        {
            var current = currentIndexes.FirstOrDefault(x => x.MongoName == index["name"].ToString());
            if (current != null && index.TryGetValue("expireAfterSeconds", out var expireAfterSeconds))
                current.ExpireAfterSeconds = expireAfterSeconds.ToInt64();
        }
        
        // Create new indexes
        foreach (var (indexProperty, indexAttribute) in indexProps)
        {
            var existing = currentIndexes.FirstOrDefault(x => x.Name == indexProperty.Name);
            if (existing != null && existing.ExpireAfterSeconds == null)
                continue;

            // [MongoTtl] was removed from the property, turn the TTL index back into a plain one
            if (existing != null)
            {
                Console.WriteLine($"Deleting TTL index {documentType.Name}.{existing.MongoName}");
                await collection.DeleteIndexAsync(existing.MongoName);
                currentIndexes.Remove(existing);
            }

            Console.WriteLine($"Creating index {documentType.Name}.{indexProperty.Name}");
            await collection.CreateIndexAsync(indexProperty.Name, indexAttribute.IsUnique);
        }

        foreach (var (ttlProperty, ttlAttribute) in ttlProps)
        {
            var expireAfterSeconds = ttlAttribute!.ExpireAfterSeconds;
            var existing = currentIndexes.FirstOrDefault(x => x.Name == ttlProperty.Name);
            if (existing != null && existing.ExpireAfterSeconds == expireAfterSeconds)
                continue;

            // Plain index on the field or a TTL with a different expiry, recreate with the new one
            if (existing != null)
            {
                Console.WriteLine($"Deleting index {documentType.Name}.{existing.MongoName} to change TTL");
                await collection.DeleteIndexAsync(existing.MongoName);
                currentIndexes.Remove(existing);
            }

            Console.WriteLine($"Creating TTL index {documentType.Name}.{ttlProperty.Name} ({expireAfterSeconds}s)");
            await collection.CreateTtlIndexAsync(ttlProperty.Name, TimeSpan.FromSeconds(expireAfterSeconds));
        }

        // Delete old indexes
        foreach (var indexName in currentIndexes)
        {
            if (indexProps.Any(x => x.Info.Name == indexName.Name) || ttlProps.Any(x => x.Info.Name == indexName.Name))
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
    
    public static Task CreateTtlIndexAsync(this IMongoCollection<BsonDocument> collection, string fieldName, TimeSpan expireAfter)
    {
        var options = new CreateIndexOptions { ExpireAfter = expireAfter };
        var indexDefinition = Builders<BsonDocument>.IndexKeys.Ascending(fieldName);
        return collection.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(indexDefinition, options));
    }

    public static Task DeleteIndexAsync(this IMongoCollection<BsonDocument> collection, string indexName)
    {
        return collection.Indexes.DropOneAsync(indexName);
    }
}