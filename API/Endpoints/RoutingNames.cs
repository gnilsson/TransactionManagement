using API.Data;
using API.Endpoints.AccountEndpoints;
using System.Collections.Frozen;

namespace API.Endpoints;

public static class RoutingNames
{
    public static class Group
    {
        public const string Account = "accounts";
        public const string Transaction = "transactions";
        public const string Identity = "identity";
    }

    public static class Endpoint
    {
        public const string GetAccountById = nameof(AccountEndpoints.GetAccountById);
        public const string CreateAccount = nameof(AccountEndpoints.CreateAccount);
        public const string GetAccounts = nameof(AccountEndpoints.GetAccounts);

        public const string CreateTransaction = nameof(TransactionEndpoints.CreateTransaction);
        public const string GetTransactionById = nameof(TransactionEndpoints.GetTransactionById);
        public const string GetTransactions = nameof(TransactionEndpoints.GetTransactions);
    }

    public static class RequestArgument
    {
        public const string AccountId = "account_id";
    }

    public static class OrderingQueryArgument
    {
        public const string CreatedAt = "createdAt";
        public const string ModifiedAt = "modifiedAt";
        public const string TransactionCount = "transactionCount";
    }

    public static class ArgumentPropertyMaps
    {
        public static FrozenDictionary<string, string> OrderingQuery { get; } = new Dictionary<string, string>()
        {
            [OrderingQueryArgument.CreatedAt] = nameof(ITemporalEntity.CreatedAt),
            [OrderingQueryArgument.ModifiedAt] = nameof(ITemporalEntity.ModifiedAt),
            [OrderingQueryArgument.TransactionCount] = nameof(GetAccounts.Response.TransactionsCount)
        }.ToFrozenDictionary();
    }
}
