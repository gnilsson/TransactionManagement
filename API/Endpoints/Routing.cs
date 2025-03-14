using API.Data;
using API.Endpoints.TransactionEndpoints;
using API.Features;
using API.Misc;
using System.Collections.Frozen;

namespace API.Endpoints;

public static class Routing
{
    public static class GroupName
    {
        public const string Account = "accounts";
        public const string Transaction = "transactions";
        public const string Identity = "identity";
    }

    public static class EndpointName
    {
        public const string GetAccountById = nameof(AccountEndpoints.GetAccountById);
        public const string CreateAccount = nameof(AccountEndpoints.CreateAccount);
        public const string GetAccounts = nameof(AccountEndpoints.GetAccounts);

        public const string CreateTransaction = nameof(TransactionEndpoints.CreateTransaction);
        public const string GetTransactionById = nameof(TransactionEndpoints.GetTransactionById);
        public const string GetTransactions = nameof(TransactionEndpoints.GetTransactions);
    }

    public static class PropertyArgumentName
    {
        public const string AccountId = "account_id";
    }

    public static class PropertyQueryArgumentName
    {
        public const string CreatedAt = "createdAt";
        public const string ModifiedAt = "modifiedAt";
    }

    public static class Entity<TResponse>
    {
        static Entity()
        {
            string[] availableSortOrders =
            [
                PropertyQueryArgumentName.CreatedAt,
                PropertyQueryArgumentName.ModifiedAt
            ];

            var orderQueries = new Dictionary<
                string,
                Func<IQueryable<TResponse>, IOrderedQueryable<TResponse>>>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var sortBy in availableSortOrders)
            {
                orderQueries.Add(
                    $"{sortBy}{nameof(Pagination.SortDirection.Ascending)}",
                    QueryBuilder.CreateOrderQuery<TResponse>(sortBy, true));

                orderQueries.Add(
                    $"{sortBy}{nameof(Pagination.SortDirection.Descending)}",
                    QueryBuilder.CreateOrderQuery<TResponse>(sortBy, false));
            }
            OrderQueries = orderQueries.ToFrozenDictionary();
        }
        public static FrozenDictionary<string, Func<IQueryable<TResponse>, IOrderedQueryable<TResponse>>> OrderQueries { get; }
    }

    static Routing()
    {
        string[] availableSortOrders =
        [
            PropertyQueryArgumentName.CreatedAt,
            PropertyQueryArgumentName.ModifiedAt
        ];

        var featuredEndpoints = new Dictionary<string, FeaturedEndpointMetadata>()
        {
            [$"/{GroupName.Transaction}"] = new FeaturedEndpointMetadata
            {
                GroupName = GroupName.Transaction,
                ForeignIdArgumentName = PropertyArgumentName.AccountId,
                AvailableSortOrders = FrozenSet.Create<string>(availableSortOrders),
            }
        };
        FeaturedEndpoints = featuredEndpoints.ToFrozenDictionary();
    }

    public static FrozenDictionary<string, FeaturedEndpointMetadata> FeaturedEndpoints { get; }

    public static void Initialize()
    {
        _ = Entity<GetTransactions.Response>.OrderQueries;
        _ = Entity<Account>.OrderQueries;
    }
}
