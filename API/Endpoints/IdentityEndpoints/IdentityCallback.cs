using API.Data;
using API.Identity;

namespace API.Endpoints.IdentityEndpoints;

public sealed class AuthenticationTokenData
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; set; }
    public required string TokenType { get; init; }
    public required int ExpiresIn { get; init; }
    public required int RefreshExpiresIn { get; init; }
    public int NotBeforePolicy { get; init; }
    public required Guid SessionState { get; init; }
    public required string Scope { get; init; }
}

public static class IdentityCallback
{
    public sealed class Response
    {
        public required string AccessToken { get; init; }
        public required string RefreshToken { get; set; }
        public required int ExpiresIn { get; init; }
        public required int RefreshExpiresIn { get; init; }
    }

    public sealed class Endpoint
    {
        private readonly AppDbContext _dbContext;
        private readonly AuthenticationTokenService _tokenService;

        public Endpoint(AppDbContext dbContext, AuthenticationTokenService tokenService)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
        }

        public async Task<IResult> HandleAsync(HttpContext context, CancellationToken cancellationToken)
        {
            var query = context.Request.Query;
            if (!query.TryGetValue("code", out var code) && !string.IsNullOrWhiteSpace(code))
            {
                return Results.BadRequest("Authorization code not found.");
            }

            // Determine if this is a login or registration
            var isRegistration = query.ContainsKey("registration");

            if (isRegistration)
            {
                // Handle registration-specific logic
                // For example, send a welcome email
            }

            var tokenData = await _tokenService.GetAccessTokenAsync(code.ToString(), cancellationToken);
            if (tokenData is null)
            {
                return Results.BadRequest("Failed to exchange authorization code for access token.");
            }

            var validationResult = await _tokenService.ValidateTokenAsync(tokenData.AccessToken);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest("Invalid access token.");
            }

            return Results.Ok(new Response()
            {
                AccessToken = tokenData.AccessToken,
                RefreshToken = tokenData.RefreshToken,
                ExpiresIn = tokenData.ExpiresIn,
                RefreshExpiresIn = tokenData.RefreshExpiresIn
            });
        }
    }
}
