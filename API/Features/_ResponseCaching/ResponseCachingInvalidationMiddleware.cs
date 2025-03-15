using API.Endpoints;
using Microsoft.Extensions.Caching.Hybrid;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace API.Features;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public sealed class ResponseCachingInvalidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HybridCache _cache;

    public ResponseCachingInvalidationMiddleware(RequestDelegate next, HybridCache cache)
    {
        _next = next;
        _cache = cache;
    }

    // note:
    // it might be a good idea to have a request binding middleware in the previous step
    // that way we wont read from the request body twice
    public async Task InvokeAsync(HttpContext context)
    {
        var cancellationToken = context.RequestAborted;

        var metadata = Routing.FeaturedEndpoints[context.Request.Path.Value!];

        byte[]? buffer = null;
        if (!metadata.CachingStrategy.VariantIsDefault)
        {
            // Enable buffering for the request body
            context.Request.EnableBuffering();

            // Read the request body as a byte array
            buffer = new byte[context.Request.ContentLength.GetValueOrDefault()];
            await context.Request.Body.ReadExactlyAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);

            // Reset the request body stream position so it can be read again by the next middleware
            context.Request.Body.Position = 0;
        }

        await _next(context);

        if (context.Response.StatusCode is StatusCodes.Status201Created)
        {
            var cacheTag = ResponseCaching.CreateTag(metadata, buffer);

            await _cache.RemoveByTagAsync(cacheTag, CancellationToken.None);
        }
    }
}
