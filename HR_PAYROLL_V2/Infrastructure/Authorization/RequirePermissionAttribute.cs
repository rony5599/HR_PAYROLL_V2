using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HR_PAYROLL_V2.Infrastructure.Authorization;

public static class PermissionClaims
{
    public const string ClaimType = "permission";
}

public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _permissions;

    public RequirePermissionAttribute(params string[] permissions)
    {
        _permissions = permissions;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new ChallengeResult();
            return;
        }

        if (!_permissions.Any(p => user.HasClaim(PermissionClaims.ClaimType, p)))
        {
            context.Result = new ForbidResult();
        }
    }
}
