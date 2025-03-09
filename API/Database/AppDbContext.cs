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

        var (seedUsers, seedAccounts, seedTransactions) = SeedData();

        var user = modelBuilder.Entity<User>(entity =>
        {
            entity.HasData(seedUsers);
        });

        var account = modelBuilder.Entity<Account>(entity =>
        {
            entity.HasData(seedAccounts);
        });

        var transaction = modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasData(seedTransactions);
        });

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

    private static (User[], Account[], Transaction[]) SeedData()
    {
        User[] seedUsers =
        [
            new User { Id = Guid.NewGuid(), Username = "a", Role = "admin"},
            new User { Id = Guid.NewGuid(), Username = "b", Role = "user" }
        ];
        Account[] seedAccounts =
        [
            new Account { Id = Guid.NewGuid(), Balance = 15, UserId = seedUsers[0].Id },
            new Account { Id = Guid.NewGuid(), Balance = 10, UserId = seedUsers[0].Id },
            new Account { Id = Guid.NewGuid(), Balance = 3, UserId = seedUsers[1].Id },
        ];
        Transaction[] seedTransactions =
        [
            new Transaction { Id = Guid.NewGuid(), AccountId = seedAccounts[0].Id, Amount = 5 },
            new Transaction { Id = Guid.NewGuid(), AccountId = seedAccounts[0].Id, Amount = 5 },
            new Transaction { Id = Guid.NewGuid(), AccountId = seedAccounts[0].Id, Amount = 5 },
            new Transaction { Id = Guid.NewGuid(), AccountId = seedAccounts[1].Id, Amount = 10 },
            new Transaction { Id = Guid.NewGuid(), AccountId = seedAccounts[2].Id, Amount = 2 },
            new Transaction { Id = Guid.NewGuid(), AccountId = seedAccounts[2].Id, Amount = 1 },
        ];
        return (seedUsers, seedAccounts, seedTransactions);
    }
}
