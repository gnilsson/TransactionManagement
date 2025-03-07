using API.Data;
using API.Features;
using API.Misc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using System.Text.Json;

namespace API.Endpoints.TransactionEndpoints;

public sealed class GetTransactions
{
    public sealed class Request : Pagination.Request
    {
        [FromQuery(Name = "account_id")]
        public Guid AccountId { get; init; }
    }

    public sealed class Response
    {
        public Guid TransactionId { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public sealed class CompleteResponse
    {
        public required Pagination.Data Metadata { get; init; }
        public required IAsyncEnumerable<Response> Items { get; init; }
    }

    public sealed class Endpoint
    {
        private readonly AppDbContext _dbContext;
        private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

        public Endpoint(AppDbContext appDbContext)
        {
            _dbContext = appDbContext;
        }

        public async Task<IResult> HandleAsync(Request request, HttpContext context, CancellationToken cancellationToken)
        {
            var query = _dbContext.Transactions
                .AsNoTracking()
                .Where(t => t.AccountId == request.AccountId);

            var totalCount = await query.CountAsync(cancellationToken);

            var paginationQuery = (context.Items[Pagination.Defaults.QueryKey] as Pagination.Query)!;
            var paginationData = new Pagination.Data { TotalCount = totalCount, PageNumber = paginationQuery.PageNumber };

            if (paginationQuery.Mode is Pagination.Mode.Metadata)
            {
                return Results.Ok(paginationData);
            }

            var paginated = query
                .Select(t => new Response
                {
                    TransactionId = t.Id,
                    Amount = t.Amount,
                    CreatedAt = t.CreatedAt
                })
                .Skip((paginationQuery.PageNumber - 1) * Pagination.Defaults.PageSize)
                .Take(Pagination.Defaults.PageSize)
                .OrderBy(paginationQuery.SortBy, paginationQuery.SortDirection is Pagination.SortDirection.Ascending)
                .AsAsyncEnumerable();

            if (paginationQuery.Mode is Pagination.Mode.Complete)
            {
                var completeResponse = new CompleteResponse
                {
                    Metadata = paginationData,
                    Items = paginated
                };
                return Results.Ok(completeResponse);
            }

            // note:
            // The sketchy way to do things
            if (paginationQuery.Mode is Pagination.Mode.CompleteStreaming)
            {
                return Results.Stream(async (stream) =>
                {
                    await using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });

                    writer.WriteStartObject();
                    writer.WritePropertyName("metadata");
                    JsonSerializer.Serialize(writer, paginationData);
                    writer.WritePropertyName("items");
                    writer.WriteStartArray();
                    await writer.FlushAsync(cancellationToken);

                    await foreach (var item in paginated.WithCancellation(cancellationToken))
                    {
                        JsonSerializer.Serialize(writer, item);
                        await writer.FlushAsync(cancellationToken);
                    }

                    writer.WriteEndArray();
                    writer.WriteEndObject();
                    await writer.FlushAsync(cancellationToken);
                }, MediaTypeNames.Application.Json);
            }

            return Results.Stream(async (stream) =>
            {
                await foreach (var item in paginated.WithCancellation(cancellationToken))
                {
                    await JsonSerializer.SerializeAsync(stream, item, _jsonSerializerOptions, cancellationToken);
                    await stream.FlushAsync(cancellationToken);
                }
            }, MediaTypeNames.Application.Json);
        }
    }
}
