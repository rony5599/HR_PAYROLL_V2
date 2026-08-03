using HR_PAYROLL_V2.Domain.Entities;

namespace HR_PAYROLL_V2.Infrastructure.Identity;

public record TokenPair(string AccessToken, DateTime AccessTokenExpiresAt, string RefreshToken, DateTime RefreshTokenExpiresAt);

public interface IJwtTokenService
{
    Task<TokenPair> CreateTokenPairAsync(User user, IEnumerable<string> roles, Guid? employeeId);

    /// <summary>Validates a refresh token against the cache and, if valid, consumes it (rotation).</summary>
    Task<Guid?> ConsumeRefreshTokenAsync(string refreshToken);

    Task RevokeRefreshTokenAsync(string refreshToken);
}
