using API.Endpoints.AccountEndpoints;
using API.Endpoints.TransactionEndpoints;
using API.Features;
using API.Misc;
using System.Collections.Frozen;
using System.Reflection;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace API.Endpoints;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class Routing
{
    private static IEnumerable<string> DefaultSortOrders { get; } =
    [
        RoutingNames.OrderingQueryArgument.CreatedAt,
        RoutingNames.OrderingQueryArgument.ModifiedAt,
    ];

    private sealed class Account : IEntityRouting
    {
        public IEnumerable<string>? SortOrders { get; } = [RoutingNames.OrderingQueryArgument.TransactionCount];

        public string GroupName => RoutingNames.Group.Account;

        public ResponseCaching.StrategyConfig CachingStrategy { get; } = new()
        {
            Variant = ResponseCaching.StrategyVariant.Default
        };

        public Type ResponseType { get; } = typeof(GetAccounts.Response);
    }

    private sealed class Transaction : IEntityRouting
    {
        public IEnumerable<string>? SortOrders { get; }

        public string GroupName => RoutingNames.Group.Transaction;

        public ResponseCaching.StrategyConfig CachingStrategy { get; } = new()
        {
            Variant = ResponseCaching.StrategyVariant.ForeignId,
            ArgumentName = RoutingNames.RequestArgument.AccountId
        };

        public Type ResponseType => typeof(GetTransactions.Response);
    }

    public static class Entity<TResponse>
    {
        public static void Initialize(IEnumerable<string> sortOrders)
        {
            var orderQueries = new Dictionary<string, Func<IQueryable<TResponse>, IOrderedQueryable<TResponse>>>();

            foreach (var sortOrder in sortOrders)
            {
                var sortBy = RoutingNames.ArgumentPropertyMaps.OrderingQueries[sortOrder];

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

        var getEndpointMetadatas = new Dictionary<string, GetEndpointMetadata>();

        foreach (var routing in entityRoutings)
        {
            string[] sortOrders = routing.SortOrders is null
                ? [.. DefaultSortOrders]
                : [.. DefaultSortOrders, .. routing.SortOrders];

            typeof(Entity<>)
                .MakeGenericType(routing.ResponseType)
                .GetMethod(nameof(Entity<>.Initialize))!
                .Invoke(null, [sortOrders]);

            getEndpointMetadatas.Add(
                $"/{routing.GroupName}",
                new GetEndpointMetadata
                {
                    GroupName = routing.GroupName,
                    AvailableSortOrders = FrozenSet.Create(sortOrders),
                    CachingStrategy = routing.CachingStrategy
                });
        }
        FeaturedEndpoints = getEndpointMetadatas.ToFrozenDictionary();
    }

    public static FrozenDictionary<string, GetEndpointMetadata> FeaturedEndpoints { get; }
}
