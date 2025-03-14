using API.Data;
using API.Database;
using API.Features;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace API.Endpoints.TransactionEndpoints;

public static class GetTransactions
{
    public sealed class Request : Pagination.Request
    {
        [FromQuery(Name = Routing.PropertyArgumentName.AccountId)]
        public Guid AccountId { get; init; }
    }

    public sealed class Response
    {
        public Guid TransactionId { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }

    public sealed class Endpoint : GetEndpoint<Transaction, Request, Response>
    {
        public Endpoint(AppDbContext dbContext) : base(dbContext)
        { }

        protected override Expression<Func<Transaction, Response>> GetProjectionExpression()
        {
            return t => new Response
            {
                TransactionId = t.Id,
                Amount = t.Amount,
                CreatedAt = t.CreatedAt,
                ModifiedAt = t.ModifiedAt
            };
        }

        protected override Expression<Func<Transaction, bool>> GetWhereExpression(Request request)
        {
            return x => x.AccountId == request.AccountId;
        }
    }
}


//    // note:
//    // it is possible to gain some performance by using compiled queries
//    // but the drawback is complexity and simply less flexible because you have to define each potential query
//    // could be an adoptable pattern if done right

//    //private static readonly Expression<Func<Transaction, Response>> _selectExpression = t => new Response
//    //{
//    //    TransactionId = t.Id,
//    //    Amount = t.Amount,
//    //    CreatedAt = t.CreatedAt,
//    //    ModifiedAt = t.ModifiedAt
//    //};

//    //private static readonly Func<AppDbContext, Guid, Task<int>>
//    //    _countTransactionsByAccountIdAsync =
//    //    EF.CompileAsyncQuery((AppDbContext dbContext, Guid accountId) =>
//    //        dbContext.Transactions.AsNoTracking().Count(t => t.AccountId == accountId));

//    //private static readonly Func<AppDbContext, Guid, int, int, IAsyncEnumerable<Response>>
//    //    _getTransactionsByAccountIdModifiedDescendingAsync =
//    //        EF.CompileAsyncQuery((AppDbContext dbContext, Guid accountId, int skip, int take) =>
//    //            dbContext.Transactions
//    //                .AsNoTracking()
//    //                .Where(t => t.AccountId == accountId)
//    //                .OrderByDescending(x => x.ModifiedAt)
//    //                .Skip(skip)
//    //                .Take(take)
//    //                .Select(_selectExpression));