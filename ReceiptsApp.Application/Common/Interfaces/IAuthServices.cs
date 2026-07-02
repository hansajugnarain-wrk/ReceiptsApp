namespace ReceiptsApp.Application.Common.Interfaces;

/// <summary>Issues and validates JWT access/refresh tokens. Implemented in Infrastructure.</summary>
public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string email);

    string GenerateRefreshToken();
}

/// <summary>Hashes and verifies passwords. Implemented in Infrastructure using BCrypt.</summary>
public interface IPasswordHasher
{
    string Hash(string plainTextPassword);

    bool Verify(string plainTextPassword, string passwordHash);
}

/// <summary>
/// Abstracts the system clock so handlers and unit tests don't call
/// DateTime.UtcNow directly — makes time-dependent logic deterministic to test.
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
