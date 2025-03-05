//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Diagnostics;

//namespace API.Endpoints.AccountEndpoints;

//public class RowVersionInterceptor : SaveChangesInterceptor
//{
//    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
//    {
//        eventData.Context.ChangeTracker.DetectChanges();


//        foreach (var entry in eventData.Context?.ChangeTracker.Entries() ?? [])
//        {
//            if (entry.Entity is IRowVersioned entityWithRowVersion && entry.State is EntityState.Added)
//            {
//                entityWithRowVersion.RowVersion = BitConverter.GetBytes(DateTime.UtcNow.Ticks);
//            }
//        }

//        return await base.SavedChangesAsync(eventData, result, cancellationToken);
//    }
//}

//public interface IRowVersioned
//{
//    byte[] RowVersion { get; set; }
//}
