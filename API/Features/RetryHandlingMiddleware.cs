using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;

namespace API.Features;

public sealed class RetryHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RetryHandlingMiddleware> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;
    private const string LogMessageTemplate = "A concurrency exception occurred. Time: {Time} Retry attempt: {Attempt}";

    public RetryHandlingMiddleware(RequestDelegate next, ILogger<RetryHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _retryPolicy = Policy
            .Handle<DbUpdateConcurrencyException>()
            .WaitAndRetryAsync(
                3,
                attempt => TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 100 + Random.Shared.Next(0, 100)),
                (exception, timeSpan, attempt, _) =>
                {
                    _logger.LogInformation(exception, LogMessageTemplate, timeSpan, attempt);
                });
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _retryPolicy.ExecuteAsync(async (CancellationToken ct) =>
        {
            ct.ThrowIfCancellationRequested();

            await _next(context);

        }, context.RequestAborted);
    }
}
