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
                .Skip((paginationQuery.PageNumber - 1) * Pagination.Defaults.PageSize)
                .Take(Pagination.Defaults.PageSize)
                .OrderBy(paginationQuery.SortBy, paginationQuery.SortDirection is Pagination.SortDirection.Ascending)
                .AsAsyncEnumerable();

            var paginationData = new Pagination.Data { TotalCount = totalCount, PageNumber = paginationQuery.PageNumber };

            return Results.Stream(async (stream) =>
            {
                await JsonSerializer.SerializeAsync(stream, paginationData, cancellationToken: cancellationToken);
                await foreach (var entity in paginated.WithCancellation(cancellationToken))
                {
                    await JsonSerializer.SerializeAsync(stream, paginated, cancellationToken: cancellationToken);
                    await stream.FlushAsync(cancellationToken);
                }
            }, MediaTypeNames.Application.Json);
        }
    }
}
