using API.Endpoints;
using API.Endpoints.AccountEndpoints;
using API.Endpoints.IdentityEndpoints;
using API.Endpoints.TransactionEndpoints;
using API.Identity;
using API.Misc;

namespace API.ServiceConfiguration;

public static class EndpointWebApplicationExtensions
{
    public static WebApplication MapEndpoints(this WebApplication app, IConfiguration configuration)
    {
        var accounts = app.MapGroup(Routing.GroupName.Account);
        accounts.MapPost("", async (CreateAccount.Request _, CreateAccount.Endpoint endpoint, CancellationToken cancellationToken) =>
        {
            return await endpoint.HandleAsync(cancellationToken);
        }).WithName(Routing.EndpointName.CreateAccount);

        accounts.MapGet("{accountId}", async (Guid accountId, GetAccountById.Endpoint endpoint, HttpContext context, CancellationToken cancellationToken) =>
        {
            return await endpoint.HandleAsync(accountId, cancellationToken);
        }).WithName(Routing.EndpointName.GetAccountById);

        accounts.MapGet("", async ([AsParameters] GetAccounts.Request request, HttpContext context, GetAccounts.Endpoint endpoint, CancellationToken cancellationToken) =>
        {
            return await endpoint.HandleAsync(request, context, cancellationToken);
        }).WithName(Routing.EndpointName.GetAccounts);


        var transactions = app.MapGroup(Routing.GroupName.Transaction);
        transactions.MapPost("", async (CreateTransaction.Request request, CreateTransaction.Endpoint endpoint, CancellationToken cancellationToken) =>
        {
            return await endpoint.HandleAsync(request, cancellationToken);
        }).WithName(Routing.EndpointName.CreateTransaction);

        transactions.MapGet("{transactionId}", async (Guid transactionId, GetTransactionById.Endpoint endpoint, HttpContext context, CancellationToken cancellationToken) =>
        {
            return await endpoint.HandleAsync(transactionId, cancellationToken);
        }).WithName(Routing.EndpointName.GetTransactionById);

        transactions.MapGet("", async ([AsParameters] GetTransactions.Request request, HttpContext context, GetTransactions.Endpoint endpoint, CancellationToken cancellationToken) =>
        {
            return await endpoint.HandleAsync(request, context, cancellationToken);
        }).WithName(Routing.EndpointName.GetTransactions);

        app.MapIdentityEndpoints(configuration);

        return app;
    }

    private static WebApplication MapIdentityEndpoints(this WebApplication app, IConfiguration configuration)
    {
        var keyCloakSettings = configuration.GetRequiredSection(SectionName.KeyCloakSettings).Get<KeyCloakSettings>()!;
        var host = configuration.GetRequiredSection(SectionName.Host).Get<string>()!;
        var redirectUri = $"{host}/callback";
        var loginUrl = string.Format(KeyCloakEndpoints.Login, keyCloakSettings.Authority, keyCloakSettings.ClientID, redirectUri);
        var logoutUrl = string.Format(KeyCloakEndpoints.Logout, keyCloakSettings.Authority);
        var registerUrl = string.Format(KeyCloakEndpoints.Register, keyCloakSettings.Authority, keyCloakSettings.ClientID, redirectUri);

        app.MapGet("/login", () =>
        {
            return Results.Redirect(loginUrl);
        });

        app.MapGet("/logout", () =>
        {
            return Results.Redirect(logoutUrl);
        });

        app.MapGet("/register", () =>
        {
            return Results.Redirect(registerUrl);
        });

        app.MapGet("/callback", async (HttpContext context, IdentityCallback.Endpoint endpoint, CancellationToken cancellationToken) =>
        {
            await endpoint.HandleAsync(context, cancellationToken);
        });

        return app;
    }
}
