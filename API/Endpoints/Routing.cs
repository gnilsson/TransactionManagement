using API.Endpoints.AccountEndpoints;
using API.Endpoints.TransactionEndpoints;
using API.Features;
using API.Misc;
using System.Collections.Frozen;
using System.Reflection;

namespace API.Endpoints;

public static class Routing
{
    private static IEnumerable<string> DefaultSortOrders
    {
        get =>
        [
            RoutingNames.OrderingQueryArgument.CreatedAt,
            RoutingNames.OrderingQueryArgument.ModifiedAt,
        ];
    }

    private sealed class Account : IEntityRouting
    {
        public IEnumerable<string>? SortOrders { get; } = [RoutingNames.OrderingQueryArgument.TransactionCount];

        public string GroupName { get; } = RoutingNames.Group.Account;

        public CachingStrategyConfig CachingStrategy { get; } = new()
        {
            Variant = CachingStrategyVariant.Default
        };

        public Type ResponseType { get; } = typeof(GetAccounts.Response);
    }

    private sealed class Transaction : IEntityRouting
    {
        public IEnumerable<string>? SortOrders { get; }

        public string GroupName { get; } = RoutingNames.Group.Transaction;

        public CachingStrategyConfig CachingStrategy { get; } = new()
        {
            Variant = CachingStrategyVariant.ForeignId,
            ArgumentName = RoutingNames.RequestArgument.AccountId
        };

        public Type ResponseType { get; } = typeof(GetTransactions.Response);
    }

    public static class Entity<TResponse>
    {
        public static void Initialize(IEnumerable<string> sortOrders)
        {
            var orderQueries = new Dictionary<string, Func<IQueryable<TResponse>, IOrderedQueryable<TResponse>>>();

            foreach (var sortOrder in sortOrders)
            {
                var sortBy = RoutingNames.ArgumentPropertyMaps.OrderingQuery[sortOrder];

                orderQueries.Add(
                    $"{sortBy}{nameof(Pagination.SortDirection.Ascending)}",
                    QueryBuilder.CreateOrderQuery<TResponse>(sortBy, true));

                orderQueries.Add(
                    $"{sortBy}{nameof(Pagination.SortDirection.Descending)}",
                    QueryBuilder.CreateOrderQuery<TResponse>(sortBy, false));
            }
            OrderQueries = orderQueries.ToFrozenDictionary();
        }

        public static FrozenDictionary<string, Func<IQueryable<TResponse>, IOrderedQueryable<TResponse>>> OrderQueries { get; private set; } = default!;
    }

    static Routing()
    {
        var entityRoutings = typeof(Routing).GetNestedTypes(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(t => t.IsClass && t.GetInterfaces().Contains(typeof(IEntityRouting)))
            .Select(t => (IEntityRouting)Activator.CreateInstance(t)!);

        var featuredEndpoints = new Dictionary<string, FeaturedEndpointMetadata>();

        foreach (var routing in entityRoutings)
        {
            string[] sortOrders = routing.SortOrders is null
                ? [.. DefaultSortOrders]
                : [.. DefaultSortOrders, .. routing.SortOrders];

            typeof(Entity<>)
                .MakeGenericType(routing.ResponseType)
                .GetMethod(nameof(Entity<>.Initialize))!
                .Invoke(null, [sortOrders]);

            featuredEndpoints.Add(
                $"/{routing.GroupName}",
                new FeaturedEndpointMetadata
                {
                    GroupName = routing.GroupName,
                    AvailableSortOrders = FrozenSet.Create(sortOrders),
                    CachingStrategy = routing.CachingStrategy
                });
        }
        FeaturedEndpoints = featuredEndpoints.ToFrozenDictionary();
    }

    public static FrozenDictionary<string, FeaturedEndpointMetadata> FeaturedEndpoints { get; }
}

public interface IEntityRouting
{
    string GroupName { get; }
    IEnumerable<string>? SortOrders { get; }
    CachingStrategyConfig CachingStrategy { get; }
    Type ResponseType { get; }
}

public sealed class CachingStrategyConfig
{
    public CachingStrategyVariant Variant { get; init; } = CachingStrategyVariant.Default;
    public string? ArgumentName { get; init; }
}

public enum CachingStrategyVariant : byte
{
    Default,
    ForeignId
}
