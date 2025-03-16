using API.Database;
using API.Features;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Net.Mime;
using System.Text.Json;

namespace API.Endpoints;

public abstract class GetEndpoint<TEntity, TRequest, TResponse> where TEntity : class
{
    private readonly AppDbContext _dbContext;

    protected GetEndpoint(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IResult> HandleAsync(TRequest request, HttpContext context, CancellationToken cancellationToken)
    {
        var queryable = _dbContext
            .Set<TEntity>()
            .AsNoTracking()
            .Where(GetWhereExpression(request));

        var paginationQuery = (Pagination.Query)context.Items[Pagination.Defaults.QueryKey]!;
        var paginatedQueryable = queryable
            .Select(GetProjectionExpression())
            .Skip((paginationQuery.PageNumber - 1) * paginationQuery.PageSize)
            .Take(paginationQuery.PageSize);

        var orderQuery = Routing.Entity<TResponse>.OrderQueries[$"{paginationQuery.SortBy}{paginationQuery.SortDirection}"];
        var orderedPaginatedItems = orderQuery(paginatedQueryable).AsAsyncEnumerable();

        if (paginationQuery.Mode is Pagination.Mode.Streaming)
        {
            return Results.Stream(async (stream) =>
            {
                await foreach (var item in orderedPaginatedItems.WithCancellation(cancellationToken))
                {
                    await JsonSerializer.SerializeAsync(stream, item, EndpointDefaults.JsonSerializerOptions, cancellationToken);
                    await stream.FlushAsync(cancellationToken);
                }
            }, MediaTypeNames.Application.Json);
        }

        var totalCount = await queryable.CountAsync(cancellationToken);

        var paginationData = new Pagination.Data
        {
            TotalCount = totalCount,
            PageNumber = paginationQuery.PageNumber,
            PageSize = paginationQuery.PageSize
        };
        var completeResponse = new Pagination.CompleteResponse<TResponse>
        {
            Metadata = paginationData,
            Items = orderedPaginatedItems
        };

        return Results.Json(completeResponse, EndpointDefaults.JsonSerializerOptions);
    }

    protected abstract Expression<Func<TEntity, TResponse>> GetProjectionExpression();

    protected abstract Expression<Func<TEntity, bool>> GetWhereExpression(TRequest request);
}
