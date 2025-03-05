using API.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace API.Identity;

public class RoleAuthorizationHandler : AuthorizationHandler<RoleRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
    {
        if (context.User.IsInRole(requirement.Role))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

// User can only access his own account


//public class RoleAuthorizationHandler2 : AuthorizationHandler<RoleRequirement>
//{
//    private readonly AppDbContext _dbContext;

//    public RoleAuthorizationHandler2(AppDbContext dbContext)
//    {
//        _dbContext = dbContext;
//    }

//    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
//    {
//        if (context.User.IsInRole(requirement.Role))
//        {
//            // Check if the user is trying to access their own account
//            if (requirement.Role == IdentityDefaults.Role.User)
//            {
//                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//                var resourceId = context.Resource as string; // Assuming the resource is a string representing the account ID

//                if (userIdClaim != null && resourceId != null)
//                {
//                    var user = await _dbContext.Users.Include(u => u.Accounts).FirstOrDefaultAsync(u => u.Id == userIdClaim);
//                    if (user != null && user.Accounts.Any(a => a.Id.ToString() == resourceId))
//                    {
//                        context.Succeed(requirement);
//                    }
//                }
//            }
//            else
//            {
//                context.Succeed(requirement);
//            }
//        }

//        return Task.CompletedTask;
//    }
//}