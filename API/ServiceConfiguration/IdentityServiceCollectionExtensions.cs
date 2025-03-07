using API.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Net.Mime;
using System.Text;

namespace API.ServiceConfiguration;

public static class IdentityServiceCollectionExtensions
{
    public static IServiceCollection AddIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        var keyCloakSettings = configuration.GetRequiredSection(SectionName.KeyCloakSettings).Get<KeyCloakSettings>()!;

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = keyCloakSettings.Authority;
            options.Audience = keyCloakSettings.ClientID;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = keyCloakSettings.Authority,
                ValidateAudience = true,
                ValidAudience = keyCloakSettings.ClientID,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyCloakSettings.ClientSecret))
            };
        });
        services.AddAuthorizationBuilder()
            .AddPolicy(IdentityDefaults.Authorization.Admin, policy => policy.RequireRole(IdentityDefaults.Role.Admin))
            .AddPolicy(IdentityDefaults.Authorization.User, policy => policy.RequireRole(IdentityDefaults.Role.User));

        services.AddScoped<AuthenticationTokenService>();

        services.AddHttpClient(IdentityDefaults.HttpClientName, client =>
        {
            var keyCloak = configuration.GetRequiredSection(SectionName.KeyCloakSettings).Get<KeyCloakSettings>()!;
            client.BaseAddress = new Uri(keyCloak.Authority);
            client.DefaultRequestHeaders.Add("Accept", MediaTypeNames.Application.Json);
        });

        return services;
    }
}
