using System.Collections.Concurrent;
using System.Reflection;
using Aco228.MongoDb.Models;
using Aco228.MongoDb.Models.Attributes;

namespace Aco228.MongoDb.Helpers;

public class MongoDocumentPropertyEntry
{
    public PropertyInfo PropertyInfo { get; set; }
    public ProjectMapAttribute? ProjectMapAttribute { get; set; }
    public string ColumnName => ProjectMapAttribute?.PropertyName ?? PropertyInfo.Name;
}

public static class MongoDocumentPropertyMap
{
    private static readonly ConcurrentDictionary<Type, List<MongoDocumentPropertyEntry>> _documentProperties = new();

    public static IEnumerable<MongoDocumentPropertyEntry> MapThrough(Type type)
    {
        if (!typeof(MongoDocumentInternal).IsAssignableFrom(type))
            throw new InvalidOperationException($"Type '{type.Name}' must inherit from MongoDocumentInternal");

        var properties = _documentProperties.GetOrAdd(type, t => BuildPropertyMap(t));
        return properties;
    }

    private static List<MongoDocumentPropertyEntry> BuildPropertyMap(Type type)
    {
        return type.GetProperties( BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite)
            .Select(prop => new MongoDocumentPropertyEntry
            {
                PropertyInfo = prop,
                ProjectMapAttribute = prop.GetCustomAttribute<ProjectMapAttribute>()
            })
            .ToList();
    }
}