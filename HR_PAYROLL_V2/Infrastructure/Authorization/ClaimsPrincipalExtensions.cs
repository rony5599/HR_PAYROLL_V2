using System.Security.Claims;

namespace HR_PAYROLL_V2.Infrastructure.Authorization;

public static class ClaimsPrincipalExtensions
{
    public static Guid CurrentUserId(this ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public static Guid? CurrentEmployeeId(this ClaimsPrincipal user) =>
        Guid.TryParse(user.FindFirstValue("EmployeeId"), out var id) ? id : null;

    public static bool IsAdministrator(this ClaimsPrincipal user) =>
        user.IsInRole("SuperAdministrator") || user.IsInRole("CompanyAdministrator") || user.IsInRole("HRAdministrator");
}
