using API.ExceptionHandling;
using API.ServiceConfiguration;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDatabase(builder.Configuration, builder.Environment.IsDevelopment());

builder.Services.AddScoped<ExceptionHandler>();

builder.Services.AddEndpoints();

builder.Services.AddHealthChecks();

builder.Services.AddHybridCache();

builder.Services.AddIdentity(builder.Configuration);

builder.Services.AddAuditing();

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
