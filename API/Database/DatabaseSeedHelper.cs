using API.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Database;

public static class DatabaseSeedHelper
{
    public static void SeedData(DbContext context)
    {
        var (seedUsers, seedAccounts, seedTransactions) = CreateSeedData();

        var users = context.Set<User>();
        if (!users.Any())
        {
            users.AddRange(seedUsers);
            context.SaveChanges();
        }

        var accounts = context.Set<Account>();
        if (!accounts.Any())
        {
            accounts.AddRange(seedAccounts);
            context.SaveChanges();
        }

        var transactions = context.Set<Transaction>();
        if (!transactions.Any())
        {
            transactions.AddRange(seedTransactions);
            context.SaveChanges();
        }
    }

    public static async Task SeedDataAsync(DbContext context, CancellationToken cancellationToken)
    {
        var (seedUsers, seedAccounts, seedTransactions) = CreateSeedData();

        var users = context.Set<User>();
        if (!await users.AnyAsync(cancellationToken))
        {
            users.AddRange(seedUsers);
            await context.SaveChangesAsync(cancellationToken);
        }

        var accounts = context.Set<Account>();
        if (!await accounts.AnyAsync(cancellationToken))
        {
            accounts.AddRange(seedAccounts);
            await context.SaveChangesAsync(cancellationToken);
        }

        var transactions = context.Set<Transaction>();
        if (!await transactions.AnyAsync(cancellationToken))
        {
            transactions.AddRange(seedTransactions);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static (User[], Account[], Transaction[]) CreateSeedData()
    {
        User[] seedUsers =
        [
            new User { Id = Guid.NewGuid(), Username = "a", Role = "admin" },
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
