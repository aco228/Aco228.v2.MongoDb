using System.Linq.Expressions;
using Aco228.MongoDb.Models;

namespace Aco228.MongoDb.Extensions.RepoExtensions;

public static class MongoRepoDeleteBySpecificationExtensions
{
    public static async Task DeleteAsync<TDocument, TProjection>(
        this LoadSpecification<TDocument, TProjection> spec,
        Expression<Func<TDocument, bool>>? filter = null)
        where TDocument : MongoDocument
        where TProjection : class
    {
        spec.FilterBy(filter);
        var filters = spec.BuildFilter();
        await spec.Repo.GetCollection().DeleteManyAsync(filters);
    }
}
