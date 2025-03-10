using API.Data;
using API.ExceptionHandling;
using API.Misc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Collections.Concurrent;
using System.Text.Json;

namespace API.Database;

public sealed class AuditingSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IBackgroundTaskQueueWriter<IEnumerable<AuditLog>> _queueWriter;
    private readonly ConcurrentBag<AuditLog> _auditLogs = [];

    public AuditingSaveChangesInterceptor(IBackgroundTaskQueueWriter<IEnumerable<AuditLog>> queueWriter)
    {
        _queueWriter = queueWriter;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        ThrowHelper.ThrowIfNull(context, "Context not found.");

        foreach (var auditLog in YieldAuditLog(context))
        {
            _auditLogs.Add(auditLog);
        }

        return ValueTask.FromResult(result);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        await _queueWriter.QueueAsync([ .._auditLogs], cancellationToken);

        _auditLogs.Clear();

        return result;
    }

    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        _auditLogs.Clear();

        return Task.CompletedTask;
    }

    private static IEnumerable<AuditLog> YieldAuditLog(DbContext context)
    {
        foreach (var entry in context.ChangeTracker
            .Entries()
            .Where(x => x.Entity is not AuditLog && x.State is EntityState.Added or EntityState.Modified))
        {
            var keyValues = entry.Properties
                .Where(p => p.Metadata.IsPrimaryKey())
                .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);

            var oldValues = entry.State is EntityState.Modified
                ? entry.Properties.Where(p => p.IsModified).ToDictionary(p => p.Metadata.Name, p => p.OriginalValue)
                : null;

            var newValues = entry.State is EntityState.Modified
                ? entry.Properties.Where(p => p.IsModified).ToDictionary(p => p.Metadata.Name, p => p.CurrentValue)
                : entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);

            var auditLog = new AuditLog
            {
                TableName = entry.Entity.GetType().Name,
                Action = entry.State.ToString(),
                KeyValues = JsonSerializer.Serialize(keyValues),
                OldValues = oldValues is not null ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = JsonSerializer.Serialize(newValues),
                UserId = "system", // Replace with actual user ID if available
                Timestamp = DateTime.UtcNow
            };

            yield return auditLog;
        }
    }
}
