using API.Endpoints.AccountEndpoints;
using API.Endpoints.IdentityEndpoints;
using API.Endpoints.TransactionEndpoints;

namespace API.ServiceConfiguration;

public static class EndpointServiceCollectionExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services)
    {
        services.AddScoped<CreateTransaction.Endpoint>();
        services.AddScoped<GetTransactionById.Endpoint>();
        services.AddScoped<GetTransactions.Endpoint>();

        services.AddScoped<CreateAccount.Endpoint>();
        services.AddScoped<GetAccountById.Endpoint>();

        services.AddScoped<IdentityCallback.Endpoint>();

        return services;
    }
}
