using API.Database;
using Microsoft.EntityFrameworkCore;

namespace API.Endpoints.AccountEndpoints;

public sealed class GetAccounts
{
    public sealed class Request { }
    public sealed class Response
    {
        public Guid Id { get; init; }
    }

    public sealed class Endpoint
    {
        private readonly AppDbContext _dbContext;

        public Endpoint(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IResult> HandleAsync(Request request, HttpContext context, CancellationToken cancellationToken)
        {
            var accounts = await _dbContext.Accounts
                .OrderByDescending(x => x.Transactions.Count)
                .Select(a => new Response { Id = a.Id })
                .ToArrayAsync(cancellationToken);

            return Results.Json(accounts, EndpointDefaults.JsonSerializerOptions);
        }
    }
}
