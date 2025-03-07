using API.Data;

namespace API.Endpoints.AccountEndpoints;

public static class GetAccountById
{
    public sealed class Request
    {
        public required Guid Id { get; init; }
    }

    public sealed class Response
    {
        public required Guid Id { get; init; }
        public decimal Balance { get; init; }
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
            var account = await _appDbContext.Accounts.FindAsync([requestId], cancellationToken);
            if (account is null) return Results.NotFound();

            var response = new Response
            {
                Id = account.Id,
                Balance = account.Balance,
            };
            return Results.Ok(response);
        }
    }
}
