namespace Aco228.MongoDb.Models.Attributes;

public class MongoIndexAttribute : Attribute
{
    public bool IsUnique { get; set; }
}