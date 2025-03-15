using API.Endpoints;
using API.ExceptionHandling;
using API.Features;
using API.Logging;

namespace API.ServiceConfiguration;

public static class MiddlewareWebApplicationExtensions
{
    public static WebApplication UseMiddlewares(this WebApplication app)
    {
        app.UseMiddleware<DiagnosticsMiddleware>();

        app.UseExceptionHandler(errApp =>
        {
            errApp.Run(async context =>
            {
                await using var scope = errApp.ApplicationServices.CreateAsyncScope();
                var exceptionHandler = scope.ServiceProvider.GetRequiredService<ExceptionHandler>();

                await exceptionHandler.HandleExceptionAsync(context);
            });
        });

        app.UseWhen(context => context.Request.Method == HttpMethods.Post, appBuilder =>
        {
            appBuilder.UseMiddleware<RetryHandlingMiddleware>();
        });

        app.UseWhen(context =>
        {
            return context.Request.Path.HasValue
            && Routing.FeaturedEndpoints.ContainsKey(context.Request.Path.Value)
            && context.Request.RouteValues.Count == 0
            && context.Request.Method == HttpMethods.Get;
        }, appBuilder =>
        {
            appBuilder.UseMiddleware<Pagination.RequestBindingMiddleware>();
            appBuilder.UseMiddleware<ResponseCachingMiddleware>();
        });

        app.UseWhen(context =>
        {
            return context.Request.Path.HasValue
            && Routing.FeaturedEndpoints.ContainsKey(context.Request.Path.Value)
            && context.Request.Method == HttpMethods.Post;
        }, appBuilder =>
        {
            appBuilder.UseMiddleware<ResponseCachingInvalidationMiddleware>();
        });

        return app;
    }
}
