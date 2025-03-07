using API.Data;
using API.ExceptionHandling;
using API.Identity;
using API.ServiceConfiguration;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Net.Mime;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContextPool<AppDbContext>((sp, options) =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection"));
    // note:
    // couldn't get the rowinterceptor to work with sqlite, instead there is a trigger in the initial migration
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

builder.Services.AddHttpClient(IdentityDefaults.HttpClientName, client =>
{
    var keyCloak = builder.Configuration.GetRequiredSection(SectionName.KeyCloakSettings).Get<KeyCloakSettings>()!;
    client.BaseAddress = new Uri(keyCloak.Authority);
    client.DefaultRequestHeaders.Add("Accept", MediaTypeNames.Application.Json);
});

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


// notes:
// finish auth
// add auditing
//
