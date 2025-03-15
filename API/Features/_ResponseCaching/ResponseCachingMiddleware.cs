using API.Endpoints;
using Microsoft.Extensions.Caching.Hybrid;
using System.Net.Mime;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace API.Features;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public sealed class ResponseCachingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HybridCache _cache;

    public ResponseCachingMiddleware(RequestDelegate next, HybridCache cache)
    {
        _next = next;
        _cache = cache;
    }

    // note:
    // there are some things that can be done regarding the effectivity of the cache
    public async Task InvokeAsync(HttpContext context)
    {
        var cancellationToken = context.RequestAborted;

        var metadata = Routing.FeaturedEndpoints[context.Request.Path.Value!];
        var foreignId = context.Request.Query[metadata.CachingStrategy.ArgumentName!].ToString(); // <
        var paginationQuery = (Pagination.Query)context.Items[Pagination.Defaults.QueryKey]!;

        var cacheKey = string.Format(
            ResponseCaching.Keys.PaginatedOnForeignId,
            metadata.GroupName,
            foreignId,
            paginationQuery.PageNumber,
            paginationQuery.PageSize,
            paginationQuery.SortBy,
            paginationQuery.SortDirection,
            paginationQuery.Mode);

        var cachedResponse = await _cache.GetOrCreateAsync(cacheKey, async ct =>
         {
             ct.ThrowIfCancellationRequested();

             var originalBody = context.Response.Body;
             await using var cachingStream = new MemoryStream();
             context.Response.Body = new ResponseCachingTeeStream(context.Response.Body, cachingStream);

             try
             {
                 await _next(context);
             }
             finally
             {
                 context.Response.Body = originalBody;
             }

             cachingStream.Seek(0, SeekOrigin.Begin);
             return await new StreamReader(cachingStream).ReadToEndAsync(CancellationToken.None);
         },
         tags: [string.Format(ResponseCaching.Tags.GroupNameWithIdentifier, metadata.GroupName, foreignId)],
         cancellationToken: cancellationToken);

        if (!context.Response.HasStarted)
        {
            context.Response.ContentType = MediaTypeNames.Application.Json;
            await context.Response.WriteAsync(cachedResponse, cancellationToken);
        }
    }
}
