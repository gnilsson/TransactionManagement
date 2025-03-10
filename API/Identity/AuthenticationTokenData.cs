namespace API.Identity;

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
