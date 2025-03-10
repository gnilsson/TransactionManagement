using API.Data;
using API.Database;
using Microsoft.EntityFrameworkCore;

namespace API.Endpoints.AccountEndpoints;

public static class CreateAccount
{
    public sealed class Request
    { }

    public sealed class Endpoint
    {
        private readonly AppDbContext _appDbContext;

        public Endpoint(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IResult> HandleAsync(CancellationToken cancellationToken)
        {
            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Username == "a", cancellationToken);
            if (user is null) return Results.NotFound();

            var account = new Account { UserId = user.Id };
            _appDbContext.Accounts.Add(account);
            await _appDbContext.SaveChangesAsync(cancellationToken);

            return Results.CreatedAtRoute(
                Routing.EndpointName.GetAccountById,
                new { accountId = account.Id },
                new GetAccountById.Response { Id = account.Id, Balance = account.Balance });
        }
    }
}
