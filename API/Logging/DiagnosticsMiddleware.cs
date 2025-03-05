using System.Diagnostics;

namespace API.Logging;

public sealed class DiagnosticsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DiagnosticsMiddleware> _logger;

    public DiagnosticsMiddleware(RequestDelegate next, ILogger<DiagnosticsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Handling request: {Method} {Path}", context.Request.Method, context.Request.Path);

        await _next(context);

        stopwatch.Stop();

        _logger.LogInformation(
            "Finished handling request. Status code: {StatusCode}. Time taken: {ElapsedMilliseconds} ms",
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
}
