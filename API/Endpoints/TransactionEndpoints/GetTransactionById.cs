using API.Data;

namespace API.Endpoints.TransactionEndpoints;

public static class GetTransactionById
{
    public sealed class Request
    {
        public required Guid Id { get; init; }
    }

    public sealed class Response
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public sealed class Endpoint
    {
        private readonly AppDbContext _appDbContext;

        public Endpoint(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IResult> HandleAsync(Guid requestId, CancellationToken cancellationToken)
        {
            var transaction = await _appDbContext.Transactions.FindAsync([requestId], cancellationToken);
            if (transaction is null) return Results.NotFound();

            var response = new Response
            {
                Id = transaction.Id,
                AccountId = transaction.AccountId,
                Amount = transaction.Amount,
                CreatedAt = transaction.CreatedAt
            };
            return Results.Ok(response);
        }
    }
}
