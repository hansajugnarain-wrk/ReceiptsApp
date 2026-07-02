using Microsoft.EntityFrameworkCore;
using ReceiptsApp.Domain.Entities;
using ReceiptsApp.Domain.Interfaces;
using ReceiptsApp.Infrastructure.Persistence;

namespace ReceiptsApp.Infrastructure.Identity;

/// <summary>EF Core implementation of IUserRepository, backed by UsersDbContext.</summary>
public class UserRepository : IUserRepository
{
    private readonly UsersDbContext _dbContext;

    public UserRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower(), cancellationToken);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        await _dbContext.Users.AddAsync(user, cancellationToken);

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Update(user);
        return Task.CompletedTask;
    }
}
