using API.Endpoints;
using API.Endpoints.AccountEndpoints;
using API.Endpoints.IdentityEndpoints;
using API.Endpoints.TransactionEndpoints;
using API.Identity;

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


        var keyCloakSettings = configuration.GetRequiredSection(KeyCloakSettings.SectionName).Get<KeyCloakSettings>()!;
        var redirectUri = "https://localhost:7150/callback";
        var loginUrl = $"{keyCloakSettings.Authority}/protocol/openid-connect/auth?client_id={keyCloakSettings.ClientID}&response_type=code&scope=openid&redirect_uri={redirectUri}";
        var logoutUrl = $"{keyCloakSettings.Authority}/protocol/openid-connect/logout";
        var registerUrl = $"{keyCloakSettings.Authority}/protocol/openid-connect/registrations?client_id={keyCloakSettings.ClientID}&response_type=code&scope=openid&redirect_uri={redirectUri}";
        var r = $"http://localhost:8080/realms/myrealm/protocol/openid-connect/registrations?client_id=myclient&scope=openid%20profile&redirect_uri={redirectUri}&response_type=code";
        // &response_type=code
        app.MapGet("/login", () =>
        {
            return Results.Redirect(loginUrl);
        });

        app.MapGet("/register", () =>
        {
            return Results.Redirect(r);
        });

        app.MapGet("/logout", (HttpContext context) =>
        {
            //context.Session.Clear();
            return Results.Redirect(logoutUrl);
        });


        // Sign out

        app.MapGet("/callback", async (HttpContext context, IdentityCallback.Endpoint endpoint, CancellationToken cancellationToken) =>
        {
            await endpoint.HandleAsync(context, cancellationToken);
        });


        return app;
    }
}
