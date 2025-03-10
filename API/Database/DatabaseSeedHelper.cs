using API.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Database;

public static class DatabaseSeedHelper
{
    public static void SeedData(DbContext context)
    {
        var (seedUsers, seedAccounts, seedTransactions) = CreateSeedData();

        if (!context.Set<User>().Any())
        {
            context.Set<User>().AddRange(seedUsers);
            context.SaveChanges();
        }

        if (!context.Set<Account>().Any())
        {
            context.Set<Account>().AddRange(seedAccounts);
            context.SaveChanges();
        }

        if (!context.Set<Transaction>().Any())
        {
            context.Set<Transaction>().AddRange(seedTransactions);
            context.SaveChanges();
        }
    }

    public static async Task SeedDataAsync(DbContext context, CancellationToken cancellationToken)
    {
        var (seedUsers, seedAccounts, seedTransactions) = CreateSeedData();

        if (!await context.Set<User>().AnyAsync(cancellationToken))
        {
            await context.Set<User>().AddRangeAsync(seedUsers, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        if (!await context.Set<Account>().AnyAsync(cancellationToken))
        {
            await context.Set<Account>().AddRangeAsync(seedAccounts, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        if (!await context.Set<Transaction>().AnyAsync(cancellationToken))
        {
            await context.Set<Transaction>().AddRangeAsync(seedTransactions, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static (User[], Account[], Transaction[]) CreateSeedData()
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
