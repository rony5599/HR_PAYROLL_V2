using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Infrastructure.Identity;
using HR_PAYROLL_V2.Models.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR_PAYROLL_V2.Controllers.Api;

/// <summary>Issues and refreshes JWT tokens for mobile apps and other API integrations.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    /// <summary>Authenticates with username/password and issues an access + refresh token pair.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenResponse>> Login(LoginRequest request)
    {
        var user = (await _unitOfWork.Users.FindAsync(u => u.Username == request.Username)).FirstOrDefault();
        if (user is null || !user.IsActive || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new ProblemDetails { Title = "Invalid username or password." });
        }

        var response = await BuildTokenResponseAsync(user);

        user.LastLoginAt = DateTime.UtcNow;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return Ok(response);
    }

    /// <summary>Exchanges a still-valid refresh token for a new access + refresh token pair (rotation).</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenResponse>> Refresh(RefreshRequest request)
    {
        var userId = await _jwtTokenService.ConsumeRefreshTokenAsync(request.RefreshToken);
        if (userId is null)
        {
            return Unauthorized(new ProblemDetails { Title = "Refresh token is invalid or expired." });
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
        if (user is null || !user.IsActive)
        {
            return Unauthorized(new ProblemDetails { Title = "Account is no longer active." });
        }

        var response = await BuildTokenResponseAsync(user);
        return Ok(response);
    }

    /// <summary>Revokes a refresh token, e.g. on sign-out from a mobile device.</summary>
    [HttpPost("revoke")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Revoke(RefreshRequest request)
    {
        await _jwtTokenService.RevokeRefreshTokenAsync(request.RefreshToken);
        return NoContent();
    }

    private async Task<TokenResponse> BuildTokenResponseAsync(Domain.Entities.User user)
    {
        var userRoles = await _unitOfWork.UserRoles.FindAsync(ur => ur.UserId == user.Id);
        var roles = new List<string>();
        foreach (var userRole in userRoles)
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(userRole.RoleId);
            if (role is not null)
            {
                roles.Add(role.Name);
            }
        }

        var employee = (await _unitOfWork.Employees.FindAsync(e => e.UserId == user.Id)).FirstOrDefault();

        var tokenPair = await _jwtTokenService.CreateTokenPairAsync(user, roles, employee?.Id);

        return new TokenResponse
        {
            AccessToken = tokenPair.AccessToken,
            AccessTokenExpiresAt = tokenPair.AccessTokenExpiresAt,
            RefreshToken = tokenPair.RefreshToken,
            RefreshTokenExpiresAt = tokenPair.RefreshTokenExpiresAt,
            User = new UserSummaryDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Roles = roles,
                CompanyId = user.CompanyId,
                EmployeeId = employee?.Id
            }
        };
    }
}
