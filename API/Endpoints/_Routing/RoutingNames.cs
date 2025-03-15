using API.Data;
using API.Endpoints.AccountEndpoints;
using System.Collections.Frozen;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace API.Endpoints;
#pragma warning restore IDE0130 // Namespace does not match folder structure

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
        public static FrozenDictionary<string, string> OrderingQueries { get; } = new Dictionary<string, string>()
        {
            [OrderingQueryArgument.CreatedAt] = nameof(ITemporalEntity.CreatedAt),
            [OrderingQueryArgument.ModifiedAt] = nameof(ITemporalEntity.ModifiedAt),
            [OrderingQueryArgument.TransactionCount] = nameof(GetAccounts.Response.TransactionsCount)
        }.ToFrozenDictionary();
    }
}
