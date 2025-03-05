namespace API.Identity;

public sealed class KeyCloakSettings
{
    public const string SectionName = "KeyCloak";

    public required string ClientSecret { get; init; }
    public required string ClientID { get; init; }
    public required string BaseUrl { get; init; }
    public required string Realm { get; init; }
    public string Authority => $"{BaseUrl}/auth/realms/{Realm}";
}
