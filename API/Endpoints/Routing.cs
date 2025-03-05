using API.Data;

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

    public static Dictionary<string, FeaturedEndpointMetadata> FeaturedEndpoints { get; } = new()
    {
        [$"/{GroupName.Transaction}"] = new FeaturedEndpointMetadata
        {
            GroupName = GroupName.Transaction,
            ForeignIdArgumentName = "account_id",
            AvailableSortOrders = new Dictionary<string, string>()
            {
                ["created_at"] = nameof(Transaction.CreatedAt)
            }
        }
    };
}

public sealed class FeaturedEndpointMetadata
{
    public required string GroupName { get; init; }
    public required string ForeignIdArgumentName { get; init; }
    public required IReadOnlyDictionary<string, string> AvailableSortOrders { get; init; }
}
