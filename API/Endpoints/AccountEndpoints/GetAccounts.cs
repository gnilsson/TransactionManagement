using API.Data;
using API.Database;
using System.Linq.Expressions;

namespace API.Endpoints.AccountEndpoints;

public sealed class GetAccounts
{
    public sealed class Request { }

    public sealed class Response
    {
        public required Guid Id { get; init; }
        public required DateTime CreatedAt { get; init; }
        public required DateTime ModifiedAt { get; init; }
        public required int TransactionsCount { get; init; }
    }

    public sealed class Endpoint : GetEndpoint<Account, Request, Response>
    {
        public Endpoint(AppDbContext dbContext) : base(dbContext)
        { }

        protected override Expression<Func<Account, Response>> GetProjectionExpression()
        {
            return a => new Response
            {
                Id = a.Id,
                CreatedAt = a.CreatedAt,
                ModifiedAt = a.ModifiedAt,
                TransactionsCount = a.Transactions.Count
            };
        }

        protected override Expression<Func<Account, bool>> GetQueryExpression(Request request)
        {
            return x => true;
        }
    }
}
