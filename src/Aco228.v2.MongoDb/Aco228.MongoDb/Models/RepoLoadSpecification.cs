using Aco228.MongoDb.Services;

namespace Aco228.MongoDb.Models;

public class RepoLoadSpecification<TDocument> : LoadSpecification<TDocument>
    where TDocument : MongoDocument
{
    public IMongoRepo<TDocument> Repo { get; init; }
    
    public RepoLoadSpecification(IMongoRepo<TDocument> repo)
    {
        Repo = repo;
    }
}