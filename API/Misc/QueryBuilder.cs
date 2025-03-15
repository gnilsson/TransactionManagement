using API.Endpoints;
using System.Linq.Expressions;

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

    //public static Func<IQueryable<T>, IOrderedQueryable<T>> CreateOrderQuery<T>(
    //    string propertyName,
    //    bool ascending,
    //    OrderingQueryVariant variant)
    //{
    //    // Create a parameter expression representing the input parameter of the delegate
    //    var parameter = Expression.Parameter(typeof(T), "x");

    //    // Create an expression representing the property access
    //    var property = Expression.Property(parameter, propertyName);

    //    // Get the OrderBy or OrderByDescending method
    //    var methodName = ascending ? "OrderBy" : "OrderByDescending";

    //    return variant switch
    //    {
    //        OrderingQueryVariant.EnumerableCount => CreateEnumerableCountOrderQueryVariant<T>(property, parameter, methodName),
    //        _ => CreateDefaultOrderQueryVariant<T>(property, parameter, methodName)
    //    };
    //}

    //private static Func<IQueryable<T>, IOrderedQueryable<T>> CreateEnumerableCountOrderQueryVariant<T>(
    //    MemberExpression property,
    //    ParameterExpression parameter,
    //    string methodName)
    //{
    //    // Check if the property is a collection
    //    if (!typeof(IEnumerable<>).IsAssignableFrom(property.Type.GetGenericTypeDefinition()))
    //    {
    //        throw new ArgumentException($"Property '{property.Member.Name}' is not a collection.");
    //    }

    //    // Create an expression representing the Count property of the collection
    //    var countProperty = Expression.Property(property, "Count");

    //    // Create a lambda expression representing the delegate
    //    var lambda = Expression.Lambda(countProperty, parameter);

    //    var method = typeof(Queryable).GetMethods()
    //        .First(m => m.Name == methodName && m.GetParameters().Length == 2)
    //        .MakeGenericMethod(typeof(T), countProperty.Type);

    //    // Create the source parameter for the IQueryable
    //    var sourceParameter = Expression.Parameter(typeof(IQueryable<T>), "source");

    //    // Create the method call expression
    //    var methodCall = Expression.Call(method, sourceParameter, lambda);

    //    // Compile the expression into a delegate
    //    var compiledLambda = Expression.Lambda<Func<IQueryable<T>, IOrderedQueryable<T>>>(methodCall, sourceParameter).Compile();

    //    return compiledLambda;
    //}

    //private static Func<IQueryable<T>, IOrderedQueryable<T>> CreateEnumerableCountOrderQueryVariant<T>(
    //    MemberExpression property,
    //    ParameterExpression parameter,
    //    string methodName)
    //{
    //    // Check if the property is a collection
    //    //ThrowHelper.ThrowIf(!typeof(IEnumerable<>).IsAssignableFrom(property.Type.GetGenericTypeDefinition()));

    //    // Create an expression representing the Count property of the collection
    //    var countProperty = Expression.Property(property, "Count");

    //    // Create a lambda expression representing the delegate
    //    var lambda = Expression.Lambda(countProperty, parameter);

    //    var method = typeof(Queryable).GetMethods()
    //        .First(m => m.Name == methodName && m.GetParameters().Length == 2)
    //        .MakeGenericMethod(typeof(T), countProperty.Type);

    //    // Create the source parameter for the IQueryable
    //    var sourceParameter = Expression.Parameter(typeof(IQueryable<T>), "source");

    //    // Create the method call expression
    //    var methodCall = Expression.Call(method, sourceParameter, lambda);

    //    // Compile the expression into a delegate
    //    var compiledLambda = Expression.Lambda<Func<IQueryable<T>, IOrderedQueryable<T>>>(methodCall, sourceParameter).Compile();

    //    return compiledLambda;

    //}

    private static Func<IQueryable<T>, IOrderedQueryable<T>> CreateDefaultOrderQueryVariant<T>(
        MemberExpression property,
        ParameterExpression parameter,
        string methodName)
    {
        // Create a lambda expression representing the delegate
        var lambda = Expression.Lambda(property, parameter);

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
