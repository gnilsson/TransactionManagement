﻿using API.Endpoints;
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

    // note:
    // there are some things that can be done regarding the effectivity of the cache
    public async Task InvokeAsync(HttpContext context)
    {
        var cancellationToken = context.RequestAborted;

        var metadata = Routing.FeaturedEndpoints[context.Request.Path.Value!];
        var foreignId = context.Request.Query[metadata.ForeignIdArgumentName].ToString();

        var paginationQuery = (context.Items[Pagination.Defaults.QueryKey] as Pagination.Query)!;

        var cacheKey = string.Format(
            Caching.Keys.PaginatedOnForeignId,
            metadata.GroupName,
            foreignId,
            paginationQuery.PageNumber,
            paginationQuery.PageSize,
            paginationQuery.SortBy,
            paginationQuery.SortDirection,
            paginationQuery.Mode);

        var cachedResponse = await _cache.GetOrCreateAsync(cacheKey, async ct =>
         {
             using var cachingStream = new MemoryStream();
             context.Response.Body = new ResponseCachingTeeStream(context.Response.Body, cachingStream);

             await _next(context);

             cachingStream.Seek(0, SeekOrigin.Begin);
             return await new StreamReader(cachingStream).ReadToEndAsync(ct);
         },
         tags: [string.Format(Caching.Tags.GroupNameWithIdentifier, metadata.GroupName, foreignId)],
         cancellationToken: cancellationToken);

        if (!context.Response.HasStarted)
        {
            context.Response.ContentType = MediaTypeNames.Application.Json;
            await context.Response.WriteAsync(cachedResponse, cancellationToken);
        }
    }
}
