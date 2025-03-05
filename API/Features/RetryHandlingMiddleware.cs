using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;

namespace API.Features;

public sealed class RetryHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RetryHandlingMiddleware> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;
    private const string LogMessageTemplate = "A concurrency exception occurred. Time: {Time} Retry attempt: {Attempt}";

    public RetryHandlingMiddleware(RequestDelegate next, ILogger<RetryHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _retryPolicy = Policy
            .Handle<DbUpdateConcurrencyException>()
            .WaitAndRetryAsync(3, attempt =>
                TimeSpan.FromMilliseconds(
                    Math.Pow(2, attempt) * 100 + Random.Shared.Next(0, 100)),
                    (e, ts, attempt, ctx) =>
                    {
                        _logger.LogWarning(e, LogMessageTemplate, ts, attempt);
                    });
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _retryPolicy.ExecuteAsync(async (CancellationToken ct) =>
        {
            throw new DbUpdateConcurrencyException();
            ct.ThrowIfCancellationRequested();

            await _next(context);

        }, context.RequestAborted);
    }

    // note:
    // When to Use Merging Changes:
    //1.	Complex Business Logic:
    //•	If your application has complex business logic that requires resolving conflicts based on specific rules, merging changes can be more appropriate.For example, if multiple users can update different parts of an entity, you may want to merge their changes rather than discarding one user's changes.
    //2.	Data Integrity:
    //•	When data integrity is critical, and you need to ensure that all changes are preserved, merging changes can help maintain a consistent state. This is especially important in collaborative applications where multiple users are working on the same data.
    //3.	User Experience:
    //•	If you want to provide a better user experience by minimizing the chances of losing user changes, merging changes can be beneficial.This approach allows you to handle conflicts gracefully and present users with options to resolve conflicts.
    //private async Task OnDbUpdateConcurrencyRetryAsync(Exception exception, TimeSpan timeSpan, int attempt, Context ctx)
    //{
    //    var ex = (exception as DbUpdateConcurrencyException)!;
    //    _logger.LogWarning(exception, LogMessageTemplate, timeSpan, attempt);
    //    foreach (var entry in ex.Entries)
    //    {
    //        //if (entry.Entity is IRowVersioned entity)
    //        {
    //            // Fetch the latest state of the entity from the database
    //            var dbContext = entry.Context;
    //            var entityType = entry.Metadata.ClrType;
    //            var primaryKey = entry.Metadata.FindPrimaryKey();
    //            var keyValues = primaryKey.Properties.Select(p => entry.Property(p.Name).CurrentValue).ToArray();
    //            var databaseEntity = await dbContext.FindAsync(entityType, keyValues);

    //            if (databaseEntity == null)
    //            {
    //                // Handle the case where the entity no longer exists in the database
    //                _logger.LogWarning("The entity no longer exists in the database.");
    //                continue;
    //            }

    //            // Apply custom merge strategy
    //            foreach (var property in entry.Properties)
    //            {
    //                var originalValue = property.OriginalValue;
    //                var currentValue = property.CurrentValue;
    //                var databaseValue = dbContext.Entry(databaseEntity).Property(property.Metadata.Name).CurrentValue;

    //                // Example merge strategy: prefer the current value if it has changed, otherwise use the database value
    //                property.CurrentValue = currentValue.Equals(originalValue) ? databaseValue : currentValue;
    //            }

    //            // Mark the entity as modified
    //            entry.OriginalValues.SetValues(dbContext.Entry(databaseEntity).OriginalValues);
    //            entry.CurrentValues.SetValues(dbContext.Entry(databaseEntity).CurrentValues);
    //            entry.State = EntityState.Modified;
    //        }
    //    }
    //}
}
