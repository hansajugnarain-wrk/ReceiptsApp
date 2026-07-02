using ReceiptsApp.Application.Common.Interfaces;

namespace ReceiptsApp.Infrastructure.Identity;

/// <summary>Wraps BCrypt.Net so the rest of the app never imports the library directly.</summary>
public class BcryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string plainTextPassword) =>
        BCrypt.Net.BCrypt.HashPassword(plainTextPassword, WorkFactor);

    public bool Verify(string plainTextPassword, string passwordHash) =>
        BCrypt.Net.BCrypt.Verify(plainTextPassword, passwordHash);
}
