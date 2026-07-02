using Microsoft.EntityFrameworkCore;
using ReceiptsApp.Domain.Entities;

namespace ReceiptsApp.Infrastructure.Persistence;

/// <summary>
/// EF Core context for the Users/Auth bounded concern only.
/// Deliberately does not know about Receipt or Market — those belong to
/// ReceiptsApp.Infrastructure.Receipts, which has its own DbContext and
/// can point at an entirely different database/server.
/// </summary>
public class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);
    }
}
