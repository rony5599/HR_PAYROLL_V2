using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR_PAYROLL_V2.Controllers.Api;

/// <summary>
/// Base for the JSON REST API consumed by mobile apps and other integrations.
/// Always JWT-authenticated (never the cookie scheme used by the MVC views), so
/// unauthenticated calls get a 401 response instead of a redirect to /Account/Login.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    protected Guid? CurrentEmployeeId =>
        Guid.TryParse(User.FindFirstValue("EmployeeId"), out var id) ? id : null;

    protected Guid? CurrentCompanyId =>
        Guid.TryParse(User.FindFirstValue("CompanyId"), out var id) ? id : null;
}
