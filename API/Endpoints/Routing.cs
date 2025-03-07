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
            ForeignIdArgumentName = PropertyArgumentName.AccountId,
            AvailableSortOrders = new Dictionary<string, string>()
            {
                [PropertyArgumentName.CreatedAt] = nameof(PropertyArgumentName.CreatedAt)
            }
        }
    };

    public static class PropertyArgumentName
    {
        public const string AccountId = "account_id";
        public const string CreatedAt = "created_at";
    }
}
