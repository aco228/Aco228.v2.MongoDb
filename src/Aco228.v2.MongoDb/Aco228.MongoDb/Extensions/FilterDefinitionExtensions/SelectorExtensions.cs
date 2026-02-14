using System.Linq.Expressions;

namespace Aco228.MongoDb.Extensions.FilterDefinitionExtensions;

internal static class SelectorExtensions
{
    public static string GetName<TDocument, TKey>(this Expression<Func<TDocument, TKey>> selector)
    {
        var body = selector.Body is UnaryExpression unary ? unary.Operand : selector.Body;
        var name = ((MemberExpression)body).Member.Name;
        return name;
    }
}