using ReceiptsApp.Domain.Exceptions;

namespace ReceiptsApp.Domain.Entities;

/// <summary>
/// Aggregate root representing an authenticated account holder.
/// All state mutation happens through methods that enforce invariants —
/// there are no public setters, so a User can never exist in an invalid state.
/// </summary>
public class User : BaseEntity
{
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? LastLoginAtUtc { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Required by EF Core for materialization. Kept private so it can't be
    // misused to construct an invalid User from application code.
    private User() { }

    public static User Create(string email, string passwordHash, string displayName)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new DomainValidationException("A valid email address is required.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainValidationException("Password hash is required.");

        if (string.IsNullOrWhiteSpace(displayName))
            throw new DomainValidationException("Display name is required.");

        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            DisplayName = displayName.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            IsActive = true
        };
    }

    public void RecordLogin() => LastLoginAtUtc = DateTime.UtcNow;

    public void Deactivate() => IsActive = false;
}
