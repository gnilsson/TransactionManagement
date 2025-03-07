namespace API.Identity;

public sealed class KeyCloakSettings
{
    public required string ClientSecret { get; init; }
    public required string ClientID { get; init; }
    public required string BaseUrl { get; init; }
    public required string Realm { get; init; }
    public string Authority => $"{BaseUrl}/realms/{Realm}";
}

public sealed class KeyCloakEndpoints
{
    public const string Login = "{0}/protocol/openid-connect/auth?client_id={1}&response_type=code&scope=openid&redirect_uri={2}";
    public const string Logout = "{0}/protocol/openid-connect/logout";
    public const string Register = "{0}/protocol/openid-connect/registrations?client_id={1}&response_type=code&scope=openid&redirect_uri={2}&response_type=code";
}
