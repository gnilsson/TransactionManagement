namespace API.Identity;

public sealed class IdentityDefaults
{
    public sealed class Role
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }

    public sealed class Authorization
    {
        public const string Admin = "AdminRole";
        public const string User = "UserRole";
    }

    public const string HttpClientName = "TokenClient";
}
