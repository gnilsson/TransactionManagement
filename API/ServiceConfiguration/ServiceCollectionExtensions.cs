using API.Data;
using API.Database;
using API.Endpoints.AccountEndpoints;
using API.Endpoints.IdentityEndpoints;
using API.Endpoints.TransactionEndpoints;
using API.Identity;
using API.Misc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace API.ServiceConfiguration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        var connectionString = configuration.GetConnectionString(SectionName.SqliteConnection);

        services.AddDbContextPool<AppDbContext>((sp, options) =>
        {
            options.UseSqlite(connectionString, o => o.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "app"));

            var queueWriter = sp.GetRequiredService<IBackgroundTaskQueueWriter<IEnumerable<AuditLog>>>();
            options.AddInterceptors(new AuditingSaveChangesInterceptor(queueWriter));
            options.AddInterceptors(new ModifiedSaveChangesInterceptor());

            if (isDevelopment)
            {
                options.LogTo(Console.WriteLine).EnableSensitiveDataLogging();
            }
        });

        services.AddDbContextPool<AuditDbContext>((sp, options) =>
        {
            options.UseSqlite(connectionString, o => o.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "audit"));

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

    public static IServiceCollection AddAuditing(this IServiceCollection services)
    {
        services.AddSingleton<BackgroundTaskQueue<IEnumerable<AuditLog>>>();
        services.AddSingleton<IBackgroundTaskQueueReader<IEnumerable<AuditLog>>>(sp => sp.GetRequiredService<BackgroundTaskQueue<IEnumerable<AuditLog>>>());
        services.AddSingleton<IBackgroundTaskQueueWriter<IEnumerable<AuditLog>>>(sp => sp.GetRequiredService<BackgroundTaskQueue<IEnumerable<AuditLog>>>());
        services.AddHostedService<AuditingBackgroundService>();

        return services;
    }
}
