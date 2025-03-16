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
    // for example, we check if the data exists in the cache by comparing the request key with existing keys, if it isn't an exact match
    // and then we stitch together the response from the cache to match current request
    public async Task InvokeAsync(HttpContext context)
    {
        var cancellationToken = context.RequestAborted;

        var (cacheKey, cacheTag) = ResponseCaching.CreateKeyAndTag(context);

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
         tags: [cacheTag],
         cancellationToken: cancellationToken);

        if (!context.Response.HasStarted)
        {
            context.Response.ContentType = MediaTypeNames.Application.Json;
            await context.Response.WriteAsync(cachedResponse, cancellationToken);
        }
    }
}
