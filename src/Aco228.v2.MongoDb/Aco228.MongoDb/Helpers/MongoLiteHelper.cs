using System.Collections.Concurrent;
using System.Reflection;
using Aco228.MongoDb.Models;
using Aco228.MongoDb.Models.Attributes;
using MongoDB.Driver;

namespace Aco228.MongoDb.Helpers;

public static class MongoLiteHelper
{
    private static readonly ConcurrentDictionary<Type, object?> _projectionCache = new();

    public static ProjectionDefinition<TDocument>? GetLiteProjectionFor<TDocument>(List<string> includeMembers)
    {
        var type = typeof(TDocument);

        if (!typeof(MongoLite).IsAssignableFrom(type))
            return null;

        if(!includeMembers.Any())
            if (_projectionCache.TryGetValue(type, out var cached))
                return cached as ProjectionDefinition<TDocument>;

        var excludeNames = new List<string>();
        foreach (var prop in type.GetProperties())
        {
            var attr = prop.GetCustomAttribute<MongoAddAttribute>();
            if (attr == null)
                continue;

            if (includeMembers.Any(x => x.Equals(prop.Name, StringComparison.InvariantCultureIgnoreCase)))
                continue;
            
            excludeNames.Add(prop.Name);
        }

        if (!excludeNames.Any())
        {
            if(!includeMembers.Any())
                _projectionCache.TryAdd(type, null);
            return null;
        }

        ProjectionDefinition<TDocument>? projection = null;
        var builder = Builders<TDocument>.Projection;

        foreach (var name in excludeNames)
        {
            projection = projection == null
                ? builder.Exclude(name)
                : projection.Exclude(name);
        }

        _projectionCache.TryAdd(type, projection);
        return projection;
    }
}
