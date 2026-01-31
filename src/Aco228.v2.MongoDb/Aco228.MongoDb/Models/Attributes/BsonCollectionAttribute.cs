namespace Aco228.MongoDb.Models.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class BsonCollectionAttribute : Attribute
{
    public string? CollectionName { get; }
    public Type DbContextType { get; }

    public BsonCollectionAttribute(string? collectionName, Type dbContextType)
    {
        CollectionName = collectionName;
        DbContextType = dbContextType;
    }
}