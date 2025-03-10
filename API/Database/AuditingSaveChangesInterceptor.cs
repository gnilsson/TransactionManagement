using API.Data;
using API.ExceptionHandling;
using API.Misc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Collections.Concurrent;
using System.Text.Json;

namespace API.Database;

public sealed class AuditingSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IBackgroundTaskQueueWriter<IEnumerable<AuditLog>> _queueWriter;
    private readonly ConcurrentBag<(EntityEntry, EntityState)> _entries = [];

    public AuditingSaveChangesInterceptor(IBackgroundTaskQueueWriter<IEnumerable<AuditLog>> queueWriter)
    {
        _queueWriter = queueWriter;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        ThrowHelper.ThrowIfNull(context, "Context not found.");

        foreach (var entry in context.ChangeTracker.Entries().Where(x => x.State is EntityState.Added or EntityState.Modified))
        {
            _entries.Add((entry, entry.State));
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        _ = CreateAndQueueAuditLogs([.. _entries]);

        _entries.Clear();

        return ValueTask.FromResult(result);
    }

    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        _entries.Clear();

        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    private async Task CreateAndQueueAuditLogs(IEnumerable<(EntityEntry, EntityState)> entries)
    {
        var auditLogs = YieldAuditLog(entries);

        await _queueWriter.QueueAsync(auditLogs);
    }

    private static IEnumerable<AuditLog> YieldAuditLog(IEnumerable<(EntityEntry, EntityState)> entries)
    {
        foreach (var (entry, state) in entries)
        {
            if (entry.Entity is AuditLog) continue;

            var auditLog = new AuditLog
            {
                TableName = entry.Entity.GetType().Name,
                Action = state.ToString(),

                KeyValues = JsonSerializer
                .Serialize(entry.Properties.Where(p => p.Metadata.IsPrimaryKey())
                .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue)),

                OldValues = state is EntityState.Modified
                ? JsonSerializer.Serialize(entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue))
                : null,

                NewValues = JsonSerializer.Serialize(entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue)),

                UserId = "system", // Replace with actual user ID if available

                Timestamp = DateTime.UtcNow
            };

            yield return auditLog;
        }
    }
}
