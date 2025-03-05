using API.Endpoints;
using API.Misc;
using Microsoft.Extensions.Caching.Hybrid;
using System.Net.Mime;

namespace API.Features.ResponseCaching;

public sealed class ResponseCacheMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HybridCache _cache;

    public ResponseCacheMiddleware(RequestDelegate next, HybridCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var cancellationToken = context.RequestAborted;

        var metadata = Routing.FeaturedEndpoints[context.Request.Path.Value!];
        var foreignId = context.Request.Query[metadata.ForeignIdArgumentName].ToString();

        var paginationQuery = (context.Items[Pagination.Defaults.QueryKey] as Pagination.Query)!;
        var cacheKey = $"{Routing.GroupName.Transaction}_{foreignId}_{paginationQuery.PageNumber}_{paginationQuery.SortBy}_{paginationQuery.SortDirection}";

        var cachedResponse = await _cache.GetOrCreateAsync(cacheKey, async ct =>
        {
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = new TeeStream(originalBodyStream, responseBody);

            await _next(context);

            context.Response.Body = originalBodyStream;
            responseBody.Seek(0, SeekOrigin.Begin);
            var response = await new StreamReader(responseBody).ReadToEndAsync(ct);

            return response;
        },
        tags: [string.Format(CachingTags.GroupNameWithIdentifier, metadata.GroupName, foreignId)],
        cancellationToken: cancellationToken);

        if (!context.Response.HasStarted)
        {
            context.Response.ContentType = MediaTypeNames.Application.Json;
            await context.Response.WriteAsync(cachedResponse, cancellationToken);
        }
    }
}
