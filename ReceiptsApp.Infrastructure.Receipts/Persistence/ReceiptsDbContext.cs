using Microsoft.EntityFrameworkCore;
using ReceiptsApp.Domain.Entities;

namespace ReceiptsApp.Infrastructure.Receipts.Persistence;

/// <summary>
/// Dedicated EF Core context for Receipt, ReceiptItem, and Market.
/// Kept in its own assembly and its own DbContext — deliberately separate
/// from UsersDbContext — so the receipts data store can scale, be migrated,
/// or be moved to a different SQL Server instance (e.g. the Docker
/// container described in docker-compose.yml) independently of Auth.
/// This is the separation-of-concerns boundary requested: one infrastructure
/// project, one responsibility — receipts persistence only.
/// </summary>
public class ReceiptsDbContext : DbContext
{
    public ReceiptsDbContext(DbContextOptions<ReceiptsDbContext> options) : base(options) { }

    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<ReceiptItem> ReceiptItems => Set<ReceiptItem>();
    public DbSet<Market> Markets => Set<Market>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReceiptsDbContext).Assembly);
    }
}
