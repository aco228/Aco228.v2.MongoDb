using Aco228.MongoDb.Extensions.MongoDocuments;
using Aco228.MongoDb.Models;

namespace Aco228.MongoDb.Extensions.RepoExtensions;

internal static class SpecificationLoadProjectionExtensions
{
    public static TProjection ProjectSingle<TDocument, TProjection>(this object input, LoadSpecification<TDocument, TProjection> spec)
        where TDocument : MongoDocument
        where TProjection : class
    {
        if (typeof(TProjection) == typeof(TDocument))
        {
            if(spec.TrackValues)
                (input as TDocument)?.StartTracking();
            
            return input as TProjection;
        }

        if (spec.ProjectMapper == null)
            throw new InvalidOperationException($"Project mapper of LoadSpecification is null");
        
        TDocument document = input as TDocument;
        if(document == null)
            throw new InvalidOperationException($"Project document of LoadSpecification is null");
        
        return spec.ProjectMapper.CreateObjectFrom(document, spec.TrackValues);
    }
    
    public static IEnumerable<TProjection> ProjectEnumerable<TDocument, TProjection>(this IEnumerable<object> input, LoadSpecification<TDocument, TProjection> spec)
        where TDocument : MongoDocument
        where TProjection : class
    {
        if (typeof(TProjection) == typeof(TDocument))
        {
            if (spec.TrackValues)
                (input as IEnumerable<TDocument>).StartTracking();
            return input as IEnumerable<TProjection>;
        }

        if (spec.ProjectMapper == null)
            throw new InvalidOperationException($"Project mapper of LoadSpecification is null");
        
        IEnumerable<TDocument> documents = input as IEnumerable<TDocument>;
        if(documents == null)
            throw new InvalidOperationException($"Project document of LoadSpecification is null");
        
        return spec.ProjectMapper.CreateObjectsFrom(documents, spec.TrackValues);
    }
    
    public static List<TProjection> ProjectList<TDocument, TProjection>(this IEnumerable<object> input, LoadSpecification<TDocument, TProjection> spec)
        where TDocument : MongoDocument
        where TProjection : class
    {
        if(typeof(TProjection) == typeof(TDocument))
        {
            if (spec.TrackValues)
                (input as IEnumerable<TDocument>).StartTracking();
            
            return input as List<TProjection>;
        }

        if (spec.ProjectMapper == null)
            throw new InvalidOperationException($"Project mapper of LoadSpecification is null");
        
        IEnumerable<TDocument> documents = input as IEnumerable<TDocument>;
        if(documents == null)
            throw new InvalidOperationException($"Project document of LoadSpecification is null");
        
        return spec.ProjectMapper.CreateObjectsFrom(documents, spec.TrackValues).ToList();
    }
}