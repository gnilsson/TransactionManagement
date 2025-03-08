using API.ExceptionHandling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace API.Database;

public sealed class ModifiedSaveChangesInterceptor :   SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        ThrowHelper.ThrowIfNull(context);

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State is EntityState.Modified && entry.Entity is ITemporalEntity)
            {
                entry.Property(nameof(ITemporalEntity.ModifiedAt)).CurrentValue = DateTime.Now;
            }
        }

        return ValueTask.FromResult(result);
    }
}
