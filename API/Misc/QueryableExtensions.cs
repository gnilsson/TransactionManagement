using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace API.Misc;

public static class QueryableExtensions
{
    private static readonly ConcurrentDictionary<string, LambdaExpression> _expressionCache = new();

    public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName, bool ascending = true)
    {
        var lambda = _expressionCache.GetOrAdd($"{typeof(T).FullName}.{propertyName}.{ascending}", _ =>
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyName);
            var selector = Expression.Lambda(property, parameter);
            return selector;
        });

        var method = ascending ? "OrderBy" : "OrderByDescending";
        var resultExpression = Expression.Call(
            typeof(Queryable),
            method,
            [typeof(T), lambda.ReturnType],
            source.Expression,
            Expression.Quote(lambda));

        return source.Provider.CreateQuery<T>(resultExpression);
    }
}
