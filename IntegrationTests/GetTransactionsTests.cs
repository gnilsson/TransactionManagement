using API.Endpoints.AccountEndpoints;
using API.Endpoints.TransactionEndpoints;
using API.Features;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace IntegrationTests;

public sealed class GetTransactionsTests(WebApplicationFactory<Program> __factory) : IntegrationTestsBase(__factory)
{
    [Fact]
    public async Task CanGetTransactionsWithPagination()
    {
        var accountResponse = await Client.PostAsJsonAsync($"/accounts", new { });
        accountResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var account = await accountResponse.Content.ReadFromJsonAsync<GetAccountById.Response>();
        account.Should().NotBeNull();

        for (int i = 0; i < 5; i++)
        {
            var transactionResponse = await Client.PostAsJsonAsync("/transactions", new { account_id = account.Id, amount = i + 1 });
            transactionResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        // &page_number=1&sort_order=ascending&sort_by=createdAt&mode=streaming
        var getTransactionsResponse = await Client.GetAsync($"/transactions?account_id={account.Id}");
        getTransactionsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseStream = await getTransactionsResponse.Content.ReadAsStreamAsync();
        var items = await JsonSerializer.DeserializeAsyncEnumerable<GetTransactions.Response>(responseStream, topLevelValues: true).ToArrayAsync();

        items.Should().HaveCount(5);

        var getTransactionsResponse2 = await Client.GetAsync($"/transactions?account_id={account.Id}&mode=metadata");
        getTransactionsResponse2.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseStream2 = await getTransactionsResponse.Content.ReadAsStreamAsync();
        var paginationData = await JsonSerializer.DeserializeAsync<Pagination.Data>(responseStream2);

        paginationData.Should().NotBeNull();
        paginationData.TotalCount.Should().Be(5);


        var getTransactionsResponse3 = await Client.GetAsync($"/transactions?account_id={account.Id}&mode=complete");
        getTransactionsResponse3.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseStream3 = await getTransactionsResponse.Content.ReadAsStreamAsync();
        var completeResponse = await JsonSerializer.DeserializeAsync<GetTransactions.CompleteResponse>(responseStream3);

        completeResponse.Should().NotBeNull();
        var items2 = await completeResponse.Items.ToArrayAsync();
        items2.Should().HaveCount(5);
        completeResponse.Metadata.TotalCount.Should().Be(5);
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

