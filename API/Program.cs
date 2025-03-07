using API.ExceptionHandling;
using API.Features.Auditing;
using API.Identity;
using API.ServiceConfiguration;
using Scalar.AspNetCore;
using System.Net.Mime;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDatabase(builder.Configuration, builder.Environment.IsDevelopment());

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

builder.Services.AddHostedService<AuditingBackgroundService>();

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
