﻿using API.Endpoints;
using API.ExceptionHandling;
using API.Features;
using API.Features.ResponseCaching;
using API.Logging;

namespace API.ServiceConfiguration;

public static class MiddlewareWebApplicationExtensions
{
    public static WebApplication UseMiddlewares(this WebApplication app)
    {
        app.UseMiddleware<DiagnosticsMiddleware>();

        app.UseExceptionHandler(errApp =>
        {
            using var scope = errApp.ApplicationServices.CreateScope();
            var exceptionHandler = scope.ServiceProvider.GetRequiredService<ExceptionHandler>();

            errApp.Run(async context =>
            {
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
            appBuilder.UseMiddleware<ResponseCacheMiddleware>();
        });

        app.UseWhen(context =>
        {
            return context.Request.Path.HasValue
            && Routing.FeaturedEndpoints.ContainsKey(context.Request.Path.Value)
            && context.Request.Method == HttpMethods.Post;
        }, appBuilder =>
        {
            appBuilder.UseMiddleware<ResponseCacheInvalidationMiddleware>();
        });

        return app;
    }
}
