using API.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests;

public abstract class IntegrationTestsBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected HttpClient Client { get; }

    public IntegrationTestsBase(WebApplicationFactory<Program> factory)
    {
        Client = ConfigureHttpClientForApi(factory);
    }

    private static HttpClient ConfigureHttpClientForApi(WebApplicationFactory<Program> factory)
    {
        factory = factory.WithWebHostBuilder(builder =>
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

        return factory.CreateClient();
    }
}



//var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
//var jsonElements = await JsonSerializer
//    .DeserializeAsyncEnumerable<JsonElement>(responseStream, topLevelValues: true, options)
//    .ToArrayAsync();

//var paginationData = jsonElements.First().Deserialize<Pagination.Data>(options);

//var items = jsonElements.Skip(1).Select(e => e.Deserialize<GetTransactions.Response>(options)).ToArray();

//var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
//var jsonDocument = await JsonDocument.ParseAsync(responseStream, cancellationToken: default);

//var rootElement = jsonDocument.RootElement;
//var paginationData = rootElement.GetProperty("paginationData").Deserialize<Pagination.Data>(options);

//var items = rootElement.GetProperty("items").EnumerateArray().Select(e => e.Deserialize<GetTransactions.Response>(options)).ToAsyncEnumerable();

