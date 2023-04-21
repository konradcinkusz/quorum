public class AnyRoleRequirement : IAuthorizationRequirement
{
    public string[] Roles { get; }

    public AnyRoleRequirement(params string[] roles)
    {
        Roles = roles;
    }
}

public class AnyRoleAuthorizationHandler : AuthorizationHandler<AnyRoleRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AnyRoleRequirement requirement)
    {
        // Check if user is authenticated
        if (!context.User.Identity.IsAuthenticated)
        {
            return Task.CompletedTask;
        }

        // Check if user is a member of any of the specified roles
        if (requirement.Roles.Any(role => context.User.HasClaim(c => c.Type == "role" && Extensions.ExtractStringList(c.Value).Any(d => d == role))))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
