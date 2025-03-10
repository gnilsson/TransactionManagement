using API.Data;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace API.Database;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    { }

    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("app");

        var entityTypes = typeof(AppDbContext)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(x => x.PropertyType.GetGenericArguments()[0]);

        foreach (var entityType in entityTypes)
        {
            var builder = modelBuilder.Entity(entityType);
            foreach (var @interface in entityType.GetInterfaces())
            {
                if (@interface.Name is nameof(ITemporalEntity))
                {
                    builder.Property(nameof(ITemporalEntity.CreatedAt)).HasDefaultValueSql("datetime('now')");
                    builder.Property(nameof(ITemporalEntity.ModifiedAt)).HasDefaultValueSql("datetime('now')");
                    continue;
                }
                if (@interface.Name is nameof(IRowVersionedEntity))
                {
                    builder.Property(nameof(IRowVersionedEntity.RowVersion)).HasDefaultValue(0).IsRowVersion();
                    continue;
                }
                if (@interface.Name is nameof(IIdentifiableEntity))
                {
                    builder.Property(nameof(IIdentifiableEntity.Id)).ValueGeneratedOnAdd();
                }
            }
        }
    }
}
