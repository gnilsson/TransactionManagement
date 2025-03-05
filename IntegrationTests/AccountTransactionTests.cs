using API.Data;
using API.Endpoints.AccountEndpoints;
using API.Endpoints.TransactionEndpoints;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests;

public sealed class AccountTransactionTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AccountTransactionTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
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

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task ProvidesFunctionalHealthcheck()
    {
        var response = await _client.GetAsync("/ping");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CanCreateAndReadTransactionsAndAccountsWithPositiveAmounts()
    {
        var accountResponse = await _client.PostAsJsonAsync($"/accounts", new { });
        accountResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var account = await accountResponse.Content.ReadFromJsonAsync<GetAccountById.Response>();
        account.Should().NotBeNull();

        var transactionResponse = await _client.PostAsJsonAsync("/transactions", new { account_id = account.Id, amount = 7 });
        transactionResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var transaction = await transactionResponse.Content.ReadFromJsonAsync<GetTransactionById.Response>();
        transaction.Should().NotBeNull();

        var getTransactionResponse = await _client.GetAsync($"/transactions/{transaction.Id}");
        getTransactionResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetchedTransaction = await getTransactionResponse.Content.ReadFromJsonAsync<GetTransactionById.Response>();
        fetchedTransaction.Should().NotBeNull();
        fetchedTransaction.Id.Should().Be(transaction.Id);
        fetchedTransaction.AccountId.Should().Be(account.Id);
        fetchedTransaction.Amount.Should().Be(7);

        var getAccountResponse = await _client.GetAsync($"/accounts/{account.Id}");
        getAccountResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetchedAccount = await getAccountResponse.Content.ReadFromJsonAsync<GetAccountById.Response>();
        fetchedAccount.Should().NotBeNull();
        fetchedAccount.Id.Should().Be(account.Id);
        fetchedAccount.Balance.Should().Be(7);
    }

    [Fact]
    public async Task CanCreateAndReadTransactionsAndAccountsWithNegativeAmounts()
    {
        var accountResponse = await _client.PostAsJsonAsync($"/accounts", new { });
        accountResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var account = await accountResponse.Content.ReadFromJsonAsync<GetAccountById.Response>();
        account.Should().NotBeNull();

        var transactionResponse = await _client.PostAsJsonAsync("/transactions", new { account_id = account.Id, amount = 4 });
        transactionResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var transaction = await transactionResponse.Content.ReadFromJsonAsync<GetTransactionById.Response>();
        transaction.Should().NotBeNull();

        var getAccountResponse = await _client.GetAsync($"/accounts/{account.Id}");
        getAccountResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetchedAccount = await getAccountResponse.Content.ReadFromJsonAsync<GetAccountById.Response>();
        fetchedAccount.Should().NotBeNull();
        fetchedAccount.Id.Should().Be(account.Id);
        fetchedAccount.Balance.Should().Be(4);

        var negativeTransactionResponse = await _client.PostAsJsonAsync("/transactions", new { account_id = account.Id, amount = -3 });
        negativeTransactionResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var negativeTransaction = await negativeTransactionResponse.Content.ReadFromJsonAsync<GetTransactionById.Response>();
        negativeTransaction.Should().NotBeNull();

        var getUpdatedAccountResponse = await _client.GetAsync($"/accounts/{account.Id}");
        getUpdatedAccountResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedAccount = await getUpdatedAccountResponse.Content.ReadFromJsonAsync<GetAccountById.Response>();
        updatedAccount.Should().NotBeNull();
        updatedAccount.Id.Should().Be(account.Id);
        updatedAccount.Balance.Should().Be(1);
    }

    [Fact]
    public async Task CanHandleRequestsForNonExistentAccountsAndTransactions()
    {
        var accountId = Guid.NewGuid().ToString();
        var transactionId = Guid.NewGuid().ToString();

        var getAccountResponse = await _client.GetAsync($"/accounts/{accountId}");
        getAccountResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var getTransactionResponse = await _client.GetAsync($"/transactions/{transactionId}");
        getTransactionResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CanHandleInvalidRequests()
    {
        var accountId = Guid.NewGuid().ToString();

        var putTransactionResponse = await _client.PutAsJsonAsync("/transactions", new { account_id = accountId, amount = 10 });
        putTransactionResponse.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);

        var postInvalidContentTypeResponse = await _client.PostAsync("/transactions", new StringContent("<xml></xml>", System.Text.Encoding.UTF8, "application/xml"));
        postInvalidContentTypeResponse.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);

        var postMissingAccountIdResponse = await _client.PostAsJsonAsync("/transactions", new { amount = 7 });
        postMissingAccountIdResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var postMissingAmountResponse = await _client.PostAsJsonAsync("/transactions", new { account_id = accountId });
        postMissingAmountResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var postBadFormatResponse = await _client.PostAsJsonAsync("/transactions", new { account_id = 10, amount = 7 });
        postBadFormatResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
