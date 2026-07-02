using ReceiptsApp.Domain.Entities;

namespace ReceiptsApp.Domain.Interfaces;

/// <summary>
/// Persistence contract for User, owned by the Domain layer.
/// Implemented by ReceiptsApp.Infrastructure; consumed by ReceiptsApp.Application.
/// All methods are asynchronous and accept a CancellationToken so callers
/// can propagate request cancellation down to the database driver.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);

    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
}
