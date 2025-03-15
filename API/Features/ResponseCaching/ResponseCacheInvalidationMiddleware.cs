using API.Endpoints;
using API.ExceptionHandling;
using Microsoft.Extensions.Caching.Hybrid;
using System.Text.Json;

namespace API.Features.ResponseCaching;

public sealed class ResponseCacheInvalidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HybridCache _cache;

    public ResponseCacheInvalidationMiddleware(RequestDelegate next, HybridCache cache)
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

        // Enable buffering for the request body
        context.Request.EnableBuffering();

        // Read the request body as a byte array
        var buffer = new byte[context.Request.ContentLength.GetValueOrDefault()];
        await context.Request.Body.ReadExactlyAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);

        // Reset the request body stream position so it can be read again by the next middleware
        context.Request.Body.Position = 0;

        await _next(context);

        if (context.Response.StatusCode is StatusCodes.Status201Created)
        {
            var metadata = Routing.FeaturedEndpoints[context.Request.Path.Value!];
            var reader = new Utf8JsonReader(buffer);
            var foreignId = ReadForeignIdProperty(reader, metadata.CachingStrategy.ArgumentName!); // <
            var tag = string.Format(Caching.Tags.GroupNameWithIdentifier, metadata.GroupName, foreignId);

            await _cache.RemoveByTagAsync(tag, cancellationToken);
        }
    }

    private static string ReadForeignIdProperty(Utf8JsonReader reader, string foreignIdArgumentName)
    {
        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.PropertyName && reader.GetString() == foreignIdArgumentName)
            {
                reader.Read();
                return reader.GetString()!;
            }
        }
        ThrowHelper.Throw("The foreign ID property was not found in the request body.");
        return null!; // note: interesting that the DoesNotReturn attribute doesnt work when its applied last
    }
}
