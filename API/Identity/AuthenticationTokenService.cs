using API.Endpoints.IdentityEndpoints;
using API.ExceptionHandling;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;

namespace API.Identity;

public sealed class AuthenticationTokenService
{
    private readonly KeyCloakSettings _keyCloak;
    private readonly HttpClient _httpClient;

    public AuthenticationTokenService(IOptions<KeyCloakSettings> keyCloak, IHttpClientFactory httpClientFactory)
    {
        _keyCloak = keyCloak.Value;
        _httpClient = httpClientFactory.CreateClient(IdentityDefaults.HttpClientName);
    }

    public async Task<AuthenticationTokenData?> GetAccessTokenAsync(string code, CancellationToken cancellationToken)
    {
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = _keyCloak.ClientID,
            ["client_secret"] = _keyCloak.ClientSecret,
            ["grant_type"] = "authorization_code",
            ["code"] = code!,
            ["redirect_uri"] = "https://localhost:7150/callback"
        };

        var tokenEndpoint = $"{_keyCloak.Authority}/protocol/openid-connect/token";

        var tokenResponse = await _httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(parameters), cancellationToken);

        if (!tokenResponse.IsSuccessStatusCode) return null;

        var tokenContent = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
        var tokenData = JsonSerializer.Deserialize<AuthenticationTokenData>(tokenContent)!;
        return tokenData;
    }

    public async Task<TokenValidationResult> ValidateTokenAsync(string token)
    {
        var key = Encoding.ASCII.GetBytes(_keyCloak.ClientSecret);
        var tokenHandler = new JwtSecurityTokenHandler();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _keyCloak.Authority,
            ValidateAudience = true,
            ValidAudience = _keyCloak.ClientID,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        var validationResult = await tokenHandler.ValidateTokenAsync(token, validationParameters);

        return validationResult;
    }

    public async Task<string?> RenewAccessTokenAsync(HttpContext context, CancellationToken cancellationToken)
    {
        var refreshToken = context.Session.GetString("RefreshToken");
        ThrowHelper.ThrowIfNull(refreshToken, "Refresh token not found.");

        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = _keyCloak.ClientID,
            ["client_secret"] = _keyCloak.ClientSecret,
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken
        };

        var tokenEndpoint = $"{_keyCloak.Authority}/protocol/openid-connect/token";

        var tokenResponse = await _httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(parameters), cancellationToken);

        if (!tokenResponse.IsSuccessStatusCode) return null;

        var tokenContent = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
        var tokenData = JsonSerializer.Deserialize<AuthenticationTokenData>(tokenContent)!;

        return tokenData.AccessToken;
    }
}
