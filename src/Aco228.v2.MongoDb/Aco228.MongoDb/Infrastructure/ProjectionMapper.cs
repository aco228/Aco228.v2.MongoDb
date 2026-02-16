using System.Reflection;
using Aco228.MongoDb.Models;
using Aco228.MongoDb.Models.Attributes;
using MongoDB.Driver;

namespace Aco228.MongoDb.Infrastructure;

internal class ProjectionMapper<TProject, TDocument> where TDocument : MongoDocument
{
    private List<PropertyInfo> _documentProperties = new();

    private Dictionary<string, PropertyInfo> _projectionProperties = new();

    private void Prepare()
    {
        _documentProperties = typeof(TDocument).GetProperties().ToList();
        foreach (var propertyInfo in typeof(TProject).GetProperties().ToList())
        {
            var attribute = propertyInfo.GetCustomAttribute<ProjectMapAttribute>();
            if (attribute?.Ignore == true) 
                continue;

            var propertyName = attribute?.PropertyName ?? propertyInfo.Name;
            if(_projectionProperties.ContainsKey(propertyName))
                propertyName = $"{propertyName}_{Guid.NewGuid().ToString().Split("-").First()}";
            
            _projectionProperties.Add(propertyName, propertyInfo);
        }
    }

    public ProjectionDefinition<TDocument> GetProjection()
    {
        Prepare();
        
        var projection = Builders<TDocument>.Projection.Include("Id");
        foreach (var prop in _projectionProperties)
            projection = projection.Include(prop.Key);
        
        return projection;
    }

    public IEnumerable<TProject> CreateObjectsFrom(IEnumerable<TDocument> documents, bool track)
    {
        var list = new List<TProject>();
        var canBeTracked = (typeof(MongoDocumentInternal).IsAssignableFrom(typeof(TDocument)));
        
        foreach (var doc in documents)
            list.Add(CreateObjectFrom(doc, track && canBeTracked));

        return list;
    }

    public TProject CreateObjectFrom(TDocument document, bool track)
    {
        if (document == null) return default;
        var result = Activator.CreateInstance<TProject>();
        foreach (var (propertyName, propertyInfo) in _projectionProperties)
        {
            var documentProp = _documentProperties.FirstOrDefault(x => x.Name.Equals(propertyName));
            if (documentProp == null)
                continue;
            
            propertyInfo.SetValue(result, documentProp.GetValue(document));
        }
        
        if(track)
            (result as MongoDocumentInternal)?.StartTracking();

        return result;
    }
}