using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace API.Misc;

public static class QueryBuilder
{
    public static Func<IQueryable<T>, IOrderedQueryable<T>> CreateOrderQuery<T>(string propertyName, bool ascending)
    {
        // Create a parameter expression representing the input parameter of the delegate
        var parameter = Expression.Parameter(typeof(T), "x");

        // Create an expression representing the property access
        var property = Expression.Property(parameter, propertyName);

        // Create a lambda expression representing the delegate
        var lambda = Expression.Lambda(property, parameter);

        // Get the OrderBy or OrderByDescending method
        var methodName = ascending ? "OrderBy" : "OrderByDescending";

        var method = typeof(Queryable).GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), property.Type);

        // Create the source parameter for the IQueryable
        var sourceParameter = Expression.Parameter(typeof(IQueryable<T>), "source");

        // Create the method call expression
        var methodCall = Expression.Call(method, sourceParameter, lambda);

        // Compile the expression into a delegate
        var compiledLambda = Expression.Lambda<Func<IQueryable<T>, IOrderedQueryable<T>>>(methodCall, sourceParameter).Compile();

        return compiledLambda;
    }

    public static IQueryable<T> OrderByPropertyName<T>(this IQueryable<T> source, string propertyName, bool ascending = true)
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

        return source.Provider.CreateQuery<T>(resultExpression);
    }
}
