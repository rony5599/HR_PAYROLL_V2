using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Infrastructure.Caching;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HR_PAYROLL_V2.Infrastructure.Identity;

/// <summary>
/// Issues short-lived JWT access tokens plus opaque refresh tokens tracked in the
/// distributed cache (Redis), so no schema migration is needed for refresh-token storage.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private const string RefreshTokenCacheKeyPrefix = "auth:refresh:";

    private readonly JwtSettings _settings;
    private readonly ICacheService _cache;

    public JwtTokenService(IOptions<JwtSettings> settings, ICacheService cache)
    {
        _settings = settings.Value;
        _cache = cache;
    }

    public async Task<TokenPair> CreateTokenPairAsync(User user, IEnumerable<string> roles, Guid? employeeId)
    {
        var now = DateTime.UtcNow;
        var accessTokenExpiresAt = now.AddMinutes(_settings.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email)
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        if (user.CompanyId.HasValue)
        {
            claims.Add(new Claim("CompanyId", user.CompanyId.Value.ToString()));
        }
        if (employeeId.HasValue)
        {
            claims.Add(new Claim("EmployeeId", employeeId.Value.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now,
            expires: accessTokenExpiresAt,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        var refreshToken = GenerateOpaqueToken();
        var refreshTokenExpiresAt = now.AddDays(_settings.RefreshTokenDays);
        await _cache.SetAsync(RefreshTokenCacheKeyPrefix + refreshToken, (Guid?)user.Id, TimeSpan.FromDays(_settings.RefreshTokenDays));

        return new TokenPair(accessToken, accessTokenExpiresAt, refreshToken, refreshTokenExpiresAt);
    }

    public async Task<Guid?> ConsumeRefreshTokenAsync(string refreshToken)
    {
        var userId = await _cache.GetAsync<Guid?>(RefreshTokenCacheKeyPrefix + refreshToken);
        if (userId is null)
        {
            return null;
        }

        await _cache.RemoveAsync(RefreshTokenCacheKeyPrefix + refreshToken);
        return userId;
    }

    public Task RevokeRefreshTokenAsync(string refreshToken)
        => _cache.RemoveAsync(RefreshTokenCacheKeyPrefix + refreshToken);

    private static string GenerateOpaqueToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
