using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace API.Misc;

public static class QueryableExtensions
{
    private static readonly ConcurrentDictionary<string, (Delegate, Type, UnaryExpression)> _expressionCache = new();

    public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName, bool ascending = true)
    {
        var cacheKey = $"{typeof(T).FullName}.{propertyName}.{ascending}";
        var (compiledLambda, returnType, quote) = _expressionCache.GetOrAdd(cacheKey, _ =>
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyName);
            var selector = Expression.Lambda(property, parameter);
            return (selector.Compile(), selector.ReturnType, Expression.Quote(selector));
        });
        var method = ascending ? "OrderBy" : "OrderByDescending";

        var resultExpression = Expression.Call(
            typeof(Queryable),
            method,
            [typeof(T), returnType],
            source.Expression,
            quote);

        return source.Provider.CreateQuery<T>(resultExpression);
    }


    // note:
    // does it makes sense to cache the query?
    private static readonly ConcurrentDictionary<string, Delegate> _expressionCache2 = new();
    private static readonly ConcurrentDictionary<string, IQueryable> _queryCache = new();

    public static IQueryable<T> OrderBy2<T>(this IQueryable<T> source, string propertyName, bool ascending = true)
    {
        var key = $"{typeof(T).FullName}.{propertyName}.{ascending}";

        // Cache the compiled lambda expression
        var lambda = (Delegate)_expressionCache2.GetOrAdd(key, _ =>
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyName);
            var selector = Expression.Lambda(property, parameter);
            return selector.Compile();
        });

        // Cache the resulting query
        var queryKey = $"{source.Expression}.{key}";
        var cachedQuery = _queryCache.GetOrAdd(queryKey, _ =>
        {
            var method = ascending ? "OrderBy" : "OrderByDescending";
            var resultExpression = Expression.Call(
                typeof(Queryable),
                method,
                [typeof(T), lambda.GetType().GenericTypeArguments[1]],
                source.Expression,
                Expression.Quote((LambdaExpression)lambda.Target!));

            return source.Provider.CreateQuery<T>(resultExpression);
        });

        return (IQueryable<T>)cachedQuery;
    }
}
