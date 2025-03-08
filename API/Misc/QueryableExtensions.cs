using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace API.Misc;

public static class QueryableExtensions
{
    private static readonly ConcurrentDictionary<string, MethodCallExpression> _expressionCache = new();

    public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName, bool ascending = true)
    {
        var cacheKey = $"{typeof(T).FullName}.{propertyName}.{ascending}";
        var resultExpression = _expressionCache.GetOrAdd(cacheKey, _ =>
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyName);
            var selector = Expression.Lambda(property, parameter);
            var method = ascending ? "OrderBy" : "OrderByDescending";

            var resultExpression = Expression.Call(
                typeof(Queryable),
                method,
                [typeof(T), selector.ReturnType],
                source.Expression,
                Expression.Quote(selector));

            return resultExpression;
        });

        return source.Provider.CreateQuery<T>(resultExpression);
    }
}
