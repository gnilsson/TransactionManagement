using API.Data;
using API.ExceptionHandling;
using API.Misc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;

namespace API.Database;

public sealed class AuditingSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IBackgroundTaskQueueWriter<IEnumerable<AuditLog>> _queueWriter;

    public AuditingSaveChangesInterceptor(IBackgroundTaskQueueWriter<IEnumerable<AuditLog>> queueWriter)
    {
        _queueWriter = queueWriter;
    }

    public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        ThrowHelper.ThrowIfNull(context, "Context not found.");

        _ = CreateAndQueueAuditLogs(context);

        return ValueTask.FromResult(result);
    }

    private async Task CreateAndQueueAuditLogs(DbContext dbContext)
    {
        var auditLogs = YieldAuditLog(dbContext.ChangeTracker.Entries());

        await _queueWriter.QueueAsync(auditLogs);
    }

    private static IEnumerable<AuditLog> YieldAuditLog(IEnumerable<EntityEntry> entries)
    {
        foreach (var entry in entries)
        {
            if (entry.Entity is AuditLog || entry.State is EntityState.Detached || entry.State is EntityState.Unchanged) continue;

            var auditLog = new AuditLog
            {
                TableName = entry.Entity.GetType().Name,
                Action = entry.State.ToString(),

                KeyValues = JsonSerializer
                .Serialize(entry.Properties.Where(p => p.Metadata.IsPrimaryKey())
                .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue)),

                OldValues = entry.State is EntityState.Modified
                ? JsonSerializer.Serialize(entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue))
                : null,

                NewValues = entry.State is EntityState.Added or EntityState.Modified
                ? JsonSerializer.Serialize(entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue))
                : null,

                UserId = "system" // Replace with actual user ID if available
            };

            yield return auditLog;
        }
    }
}
