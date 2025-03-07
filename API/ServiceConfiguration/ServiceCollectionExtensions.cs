using API.Data;
using API.Endpoints.AccountEndpoints;
using API.Endpoints.IdentityEndpoints;
using API.Endpoints.TransactionEndpoints;
using API.Identity;
using API.Misc;
using Microsoft.EntityFrameworkCore;


namespace API.ServiceConfiguration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        var connectionString = configuration.GetConnectionString(SectionName.SqliteConnection);

        services.AddDbContextPool<AppDbContext>((sp, options) =>
        {
            options.UseSqlite(connectionString);
            // note:
            // couldn't get the rowinterceptor to work with sqlite, instead there is a trigger in the initial migration
            //options.AddInterceptors(new RowVersionInterceptor());

            var queueWriter = sp.GetRequiredService<IBackgroundTaskQueueWriter<IEnumerable<AuditLog>>>();
            options.AddInterceptors(new AuditingSavedChangesInterceptor(queueWriter));

            if (isDevelopment)
            {
                options.LogTo(Console.WriteLine).EnableSensitiveDataLogging();
            }
        });

        services.AddDbContextPool<AuditDbContext>((sp, options) =>
        {
            options.UseSqlite(connectionString);

            if (isDevelopment)
            {
                options.LogTo(Console.WriteLine).EnableSensitiveDataLogging();
            }
        });

        return services;
    }

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
