using Aco228.Common.Extensions;
using Aco228.MongoDb.Models;
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
    
    public static async Task ConfigureIndexesAsync<TDocument>(this IMongoRepo<TDocument> repo)
        where TDocument : MongoDocument
    {
        Console.WriteLine($"Configuring {typeof(TDocument).Name}");
        
        var indexProps = typeof(TDocument).GetPropertyWithAttribute<MongoIndexAttribute>();
        
        var collection = repo.GetCollection();
        var currentIndexes = new List<MongoIndex>();
        var indexes = (collection.Indexes.List() as IAsyncCursor<BsonDocument>).ToList() as List<BsonDocument>;
        foreach (var indexName in indexes.Select(x => x["name"].ToString()))
        {
            if (indexName == "_id_")
                continue;
                
            currentIndexes.Add(new()
            {
                Name = indexName.Split("_").First(),
                MongoName = indexName, 
            });
        }
        
        foreach (var (indexProperty, indexAttribute) in indexProps)
        {
            if (currentIndexes.Any(x => x.Name == indexProperty.Name))
                continue;

            Console.WriteLine($"Creating index {typeof(TDocument).Name}.{indexProperty.Name}");
            await repo.CreateIndex(indexProperty.Name, indexAttribute.IsUnique);
        }

        foreach (var indexName in currentIndexes)
        {
            if (indexProps.Any(x => x.Info.Name == indexName.Name))
                continue;
            
            Console.WriteLine($"Delete index {typeof(TDocument).Name}.{indexName}");
            repo.DeleteIndex(indexName.MongoName);
        }
    }
    
    public static Task CreateIndex<TDocument>(this IMongoRepo<TDocument> repo, string nameOfParameter, bool isUnique)
        where TDocument : MongoDocument
    {
        var options = new CreateIndexOptions() { Unique = isUnique };
        var field = new StringFieldDefinition<TDocument>(nameOfParameter);
        var indexDefinition = new IndexKeysDefinitionBuilder<TDocument>().Ascending(field);
        
        return repo.GetCollection().Indexes.CreateOneAsync(indexDefinition, options);
    }
    
    
    public static void DeleteIndex<TDocument>(this IMongoRepo<TDocument> repo, string nameOfParameter)
        where TDocument : MongoDocument
    {
        repo.GetCollection().Indexes.DropOne(nameOfParameter, CancellationToken.None);
    }
}