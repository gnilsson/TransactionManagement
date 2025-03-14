using API.Endpoints;
using API.Endpoints.TransactionEndpoints;
using API.Features;
using FluentAssertions;
using IntegrationTests.Setup;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace IntegrationTests;

public sealed class GetTransactionsTests : IClassFixture<IntegrationTestsFixture>
{
    private readonly HttpClient _client;
    private readonly Guid _accountId;
    private readonly int _amountOfTransactions;

    public GetTransactionsTests(IntegrationTestsFixture fixture)
    {
        _client = fixture.Client;
        _accountId = fixture.TestAnswers.AccountIds[0];
        _amountOfTransactions = fixture.TestAnswers.AmountOfTransactions;
    }

    [Fact]
    public async Task CanGetTransactions_WithDefaultArguments()
    {
        // Arrange
        var getTransactionsResponse = await _client.GetAsync($"/transactions?account_id={_accountId}");
        getTransactionsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        // &page_number=1&page_size=10&sort_order=ascending&sort_by=createdAt&mode=streaming << default
        var responseStream = await getTransactionsResponse.Content.ReadAsStreamAsync();
        var items = await JsonSerializer
            .DeserializeAsyncEnumerable<GetTransactions.Response>(responseStream, topLevelValues: true, EndpointDefaults.JsonSerializerOptions)
            .ToArrayAsync();

        // Assert
        items.Should().HaveCount(_amountOfTransactions);
    }

    [Fact]
    public async Task CanGetTransactions_WithModeComplete()
    {
        // Arrange
        var getTransactionsResponse = await _client.GetAsync($"/transactions?account_id={_accountId}&mode=complete");
        getTransactionsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        var completeResponse = await getTransactionsResponse.Content.ReadFromJsonAsync<Pagination.CompleteResponse<GetTransactions.Response>>(EndpointDefaults.JsonSerializerOptions);

        // Assert
        completeResponse.Should().NotBeNull();
        var items = await completeResponse.Items.ToArrayAsync();
        items.Should().HaveCount(5);
        completeResponse.Metadata.TotalCount.Should().Be(_amountOfTransactions);
    }

    [Fact]
    public async Task CanGetTransactions_SortedByModifiedAt()
    {
        // Arrange
        var getTransactionsResponse = await _client.GetAsync($"/transactions?account_id={_accountId}&sort_by=modifiedAt");
        getTransactionsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        var responseStream = await getTransactionsResponse.Content.ReadAsStreamAsync();
        var items = await JsonSerializer
            .DeserializeAsyncEnumerable<GetTransactions.Response>(responseStream, topLevelValues: true, EndpointDefaults.JsonSerializerOptions)
            .ToArrayAsync();

        // Assert
        items.Should().HaveCount(_amountOfTransactions);
        items.Should().BeInAscendingOrder(t => t!.ModifiedAt);
    }

    [Fact]
    public async Task CanGetTransactions_SortedByModifiedAtDescending()
    {
        // Arrange
        var getTransactionsResponse = await _client.GetAsync($"/transactions?account_id={_accountId}&sort_by=modifiedAt&sort_direction=descending");
        getTransactionsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        var responseStream = await getTransactionsResponse.Content.ReadAsStreamAsync();
        var items = await JsonSerializer
            .DeserializeAsyncEnumerable<GetTransactions.Response>(responseStream, topLevelValues: true, EndpointDefaults.JsonSerializerOptions)
            .ToArrayAsync();

        // Assert
        items.Should().HaveCount(_amountOfTransactions);
        items.Should().BeInDescendingOrder(t => t!.ModifiedAt);
    }
}
