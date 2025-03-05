using API.Data;
using API.ExceptionHandling;
using API.ServiceConfiguration;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContextPool<AppDbContext>((sp, options) =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection"));
    //options.AddInterceptors(new RowVersionInterceptor());

    if (builder.Environment.IsDevelopment())
    {
        options.LogTo(Console.WriteLine).EnableSensitiveDataLogging();
    }
});

builder.Services.AddScoped<ExceptionHandler>();

builder.Services.AddEndpoints();

builder.Services.AddHealthChecks();

builder.Services.AddHybridCache();

builder.Services.AddIdentity(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapHealthChecks("/ping");

app.UseMiddlewares();

app.MapEndpoints(app.Configuration);

app.Run();

public partial class Program { }
