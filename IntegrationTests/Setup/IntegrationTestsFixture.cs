using API.Data;
using API.Database;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Setup;

public sealed class IntegrationTestsFixture : IAsyncLifetime
{
    public HttpClient Client { get; private set; }
    public WebApplicationFactory<Program> Factory { get; private set; }
    public TestData TestData { get; private set; } = default!;

    public IntegrationTestsFixture()
    {
        Factory = InitializeFactory();

        Client = Factory.CreateClient();
    }

    private static WebApplicationFactory<Program> InitializeFactory()
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDbContextOptionsConfiguration<AppDbContext>));
                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                // Add a new DbContext registration with an in-memory database for testing
                services.AddDbContextPool<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });
            });
        });
    }

    public async Task InitializeAsync()
    {
        // Build the service provider
        var serviceProvider = Factory.Services.CreateScope().ServiceProvider;

        // Create a scope to obtain a reference to the database context
        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Ensure the database is created
        await db.Database.EnsureCreatedAsync();

        // Seed the database with test data
        TestData = await SeedTestDataAsync(db);
    }

    private static async Task<TestData> SeedTestDataAsync(AppDbContext dbContext)
    {
        // Add test data to the database
        var account = new Account { Id = Guid.NewGuid(), Balance = 1000 };
        dbContext.Accounts.Add(account);

        var amountOfTransactions = 5;
        for (int i = 0; i < amountOfTransactions; i++)
        {
            dbContext.Transactions.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = account.Id,
                Amount = 200,
                ModifiedAt = DateTime.UtcNow.AddHours(Random.Shared.Next(i, 100))
            });
        }

        await dbContext.SaveChangesAsync();

        return new TestData
        {
            AccountIds = [account.Id],
            AmountOfTransactions = amountOfTransactions,
        };
    }

    public Task DisposeAsync()
    {
        Client.Dispose();
        Factory.Dispose();
        return Task.CompletedTask;
    }
}