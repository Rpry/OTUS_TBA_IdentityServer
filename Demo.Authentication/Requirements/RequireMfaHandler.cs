using Demo.Authentication.Requirements;
using Microsoft.AspNetCore.Authorization;

public class RequireMfaHandler : AuthorizationHandler<RequireMfa>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        RequireMfa requirement)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));
        if (requirement == null)
            throw new ArgumentNullException(nameof(requirement));

        var amrClaim =
            context.User.Claims.FirstOrDefault(t => t.Type == "amr");

        if (amrClaim != null && amrClaim.Value == "{amr:mfa}")
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}