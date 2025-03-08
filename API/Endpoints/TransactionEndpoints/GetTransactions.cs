using API.Database;
using API.Features;
using API.Misc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using System.Text.Json;

namespace API.Endpoints.TransactionEndpoints;

public static class GetTransactions
{
    public sealed class Request : Pagination.Request
    {
        [FromQuery(Name = Routing.PropertyArgumentName.AccountId)]
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
            var paginated = query
                .Select(t => new Response
                {
                    TransactionId = t.Id,
                    Amount = t.Amount,
                    CreatedAt = t.CreatedAt
                })
                .Skip((paginationQuery.PageNumber - 1) * paginationQuery.PageSize)
                .Take(paginationQuery.PageSize)
                .OrderBy(paginationQuery.SortBy, paginationQuery.SortDirection is Pagination.SortDirection.Ascending)
                .AsAsyncEnumerable();

            if (paginationQuery.Mode is Pagination.Mode.Complete)
            {
                var paginationData = new Pagination.Data
                {
                    TotalCount = totalCount,
                    PageNumber = paginationQuery.PageNumber,
                    PageSize = paginationQuery.PageSize
                };
                var completeResponse = new CompleteResponse
                {
                    Metadata = paginationData,
                    Items = paginated
                };

                return Results.Json(completeResponse, EndpointDefaults.JsonSerializerOptions);
            }

            return Results.Stream(async (stream) =>
            {
                await foreach (var item in paginated.WithCancellation(cancellationToken))
                {
                    await JsonSerializer.SerializeAsync(stream, item, EndpointDefaults.JsonSerializerOptions, cancellationToken);
                    await stream.FlushAsync(cancellationToken);
                }
            }, MediaTypeNames.Application.Json);
        }
    }
}
