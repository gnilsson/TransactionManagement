using API.Endpoints.AccountEndpoints;
using API.Endpoints.TransactionEndpoints;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests;

public sealed class AccountTransactionTests(WebApplicationFactory<Program> __factory) : IntegrationTestsBase(__factory)
{
    [Fact]
    public async Task ProvidesFunctionalHealthcheck()
    {
        var response = await Client.GetAsync("/ping");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CanCreateAndReadTransactionsAndAccountsWithPositiveAmounts()
    {
        var accountResponse = await Client.PostAsJsonAsync($"/accounts", new { });
        accountResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var account = await accountResponse.Content.ReadFromJsonAsync<GetAccountById.Response>();
        account.Should().NotBeNull();

        var transactionResponse = await Client.PostAsJsonAsync("/transactions", new { account_id = account.Id, amount = 7 });
        transactionResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var transaction = await transactionResponse.Content.ReadFromJsonAsync<GetTransactionById.Response>();
        transaction.Should().NotBeNull();

        var getTransactionResponse = await Client.GetAsync($"/transactions/{transaction.Id}");
        getTransactionResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetchedTransaction = await getTransactionResponse.Content.ReadFromJsonAsync<GetTransactionById.Response>();
        fetchedTransaction.Should().NotBeNull();
        fetchedTransaction.Id.Should().Be(transaction.Id);
        fetchedTransaction.AccountId.Should().Be(account.Id);
        fetchedTransaction.Amount.Should().Be(7);

        var getAccountResponse = await Client.GetAsync($"/accounts/{account.Id}");
        getAccountResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetchedAccount = await getAccountResponse.Content.ReadFromJsonAsync<GetAccountById.Response>();
        fetchedAccount.Should().NotBeNull();
        fetchedAccount.Id.Should().Be(account.Id);
        fetchedAccount.Balance.Should().Be(7);
    }

    [Fact]
    public async Task CanCreateAndReadTransactionsAndAccountsWithNegativeAmounts()
    {
        var accountResponse = await Client.PostAsJsonAsync($"/accounts", new { });
        accountResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var account = await accountResponse.Content.ReadFromJsonAsync<GetAccountById.Response>();
        account.Should().NotBeNull();

        var transactionResponse = await Client.PostAsJsonAsync("/transactions", new { account_id = account.Id, amount = 4 });
        transactionResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var transaction = await transactionResponse.Content.ReadFromJsonAsync<GetTransactionById.Response>();
        transaction.Should().NotBeNull();

        var getAccountResponse = await Client.GetAsync($"/accounts/{account.Id}");
        getAccountResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetchedAccount = await getAccountResponse.Content.ReadFromJsonAsync<GetAccountById.Response>();
        fetchedAccount.Should().NotBeNull();
        fetchedAccount.Id.Should().Be(account.Id);
        fetchedAccount.Balance.Should().Be(4);

        var negativeTransactionResponse = await Client.PostAsJsonAsync("/transactions", new { account_id = account.Id, amount = -3 });
        negativeTransactionResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var negativeTransaction = await negativeTransactionResponse.Content.ReadFromJsonAsync<GetTransactionById.Response>();
        negativeTransaction.Should().NotBeNull();

        var getUpdatedAccountResponse = await Client.GetAsync($"/accounts/{account.Id}");
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

        var getAccountResponse = await Client.GetAsync($"/accounts/{accountId}");
        getAccountResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var getTransactionResponse = await Client.GetAsync($"/transactions/{transactionId}");
        getTransactionResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CanHandleInvalidRequests()
    {
        var accountId = Guid.NewGuid().ToString();

        var putTransactionResponse = await Client.PutAsJsonAsync("/transactions", new { account_id = accountId, amount = 10 });
        putTransactionResponse.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);

        var postInvalidContentTypeResponse = await Client.PostAsync("/transactions", new StringContent("<xml></xml>", System.Text.Encoding.UTF8, "application/xml"));
        postInvalidContentTypeResponse.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);

        var postMissingAccountIdResponse = await Client.PostAsJsonAsync("/transactions", new { amount = 7 });
        postMissingAccountIdResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var postMissingAmountResponse = await Client.PostAsJsonAsync("/transactions", new { account_id = accountId });
        postMissingAmountResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var postBadFormatResponse = await Client.PostAsJsonAsync("/transactions", new { account_id = 10, amount = 7 });
        postBadFormatResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
