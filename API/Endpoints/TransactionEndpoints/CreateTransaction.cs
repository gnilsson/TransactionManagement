using API.Data;
using API.ExceptionHandling;
using System.Text.Json.Serialization;

namespace API.Endpoints.TransactionEndpoints;

public static class CreateTransaction
{
    public sealed class Request
    {
        [JsonPropertyName("account_id")]
        public required Guid AccountId { get; init; }
        [JsonPropertyName("amount")]
        public required int Amount { get; init; }
    }

    public sealed class Endpoint
    {
        private readonly AppDbContext _dbContext;

        public Endpoint(AppDbContext appDbContext)
        {
            _dbContext = appDbContext;
        }

        public async Task<IResult> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var account = await _dbContext.Accounts.FindAsync([request.AccountId], cancellationToken);
            if (account is null) return Results.NotFound();

            if (account.Balance + request.Amount < 0)
            {
                return Results.UnprocessableEntity(new ErrorResponse
                {
                    StatusMessage = "Unprocessable Entity",
                    Information = "Insufficient balance."
                });
            }

            account.Balance += request.Amount;
            var transaction = new Transaction
            {
                AccountId = request.AccountId,
                Amount = request.Amount
            };
            _dbContext.Transactions.Add(transaction);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Results.CreatedAtRoute(
                Routing.EndpointName.GetTransactionById,
                new { transactionId = transaction.Id },
                new GetTransactionById.Response
                {
                    Id = transaction.Id,
                    AccountId = transaction.AccountId,
                    Amount = transaction.Amount,
                    CreatedAt = transaction.CreatedAt,
                });
        }
    }
}
