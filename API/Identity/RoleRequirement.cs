using Microsoft.AspNetCore.Authorization;

namespace API.Identity;

public sealed class RoleRequirement : IAuthorizationRequirement
{
    public string Role { get; }

    public RoleRequirement(string role)
    {
        Role = role;
    }
}
