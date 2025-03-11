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

    static Routing()
    {
        string[] availableSortOrders = [PropertyQueryArgumentName.CreatedAt, PropertyQueryArgumentName.ModifiedAt];
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

        var sortByQueries = new Dictionary<
            string,
            Func<IQueryable<GetTransactions.Response>, IOrderedQueryable<GetTransactions.Response>>>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var sortBy in availableSortOrders)
        {
            sortByQueries.Add(
                $"{sortBy}{nameof(Pagination.SortDirection.Ascending)}",
                QueryBuilder.CreateSortByQuery<GetTransactions.Response>(sortBy, true));

            sortByQueries.Add(
                $"{sortBy}{nameof(Pagination.SortDirection.Descending)}",
                QueryBuilder.CreateSortByQuery<GetTransactions.Response>(sortBy, false));
        }
        SortByQueries = sortByQueries.ToFrozenDictionary();
    }

    public static FrozenDictionary<string, FeaturedEndpointMetadata> FeaturedEndpoints { get; }
    public static FrozenDictionary<string, Func<IQueryable<GetTransactions.Response>, IOrderedQueryable<GetTransactions.Response>>> SortByQueries { get; }
}
