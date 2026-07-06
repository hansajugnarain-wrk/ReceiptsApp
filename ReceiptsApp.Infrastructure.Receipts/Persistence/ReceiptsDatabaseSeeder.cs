using Microsoft.Extensions.Logging;
using ReceiptsApp.Domain.Entities;

namespace ReceiptsApp.Infrastructure.Receipts.Persistence;

/// <summary>
/// Seeds reference data on first run (idempotent — safe to call on every
/// startup; checks existence before inserting). Markets are the only seed
/// entity because they are a short, well-known reference list that the
/// mobile app needs before any receipt can be created.
/// </summary>
public class ReceiptsDatabaseSeeder
{
    private readonly ReceiptsDbContext _dbContext;
    private readonly ILogger<ReceiptsDatabaseSeeder> _logger;

    public ReceiptsDatabaseSeeder(ReceiptsDbContext dbContext, ILogger<ReceiptsDatabaseSeeder> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (_dbContext != null && _dbContext.Markets.Any())
        {
            _logger.LogInformation("Markets already seeded — skipping.");
            return;
        }

        var markets = new[]
        {
            Market.Create("Carrefour", "Mauritius"),
            Market.Create("Winner", "Mauritius"),
            Market.Create("Super U", "Mauritius"),
            Market.Create("Jumbo", "Mauritius"),
            Market.Create("Shoprite", "Mauritius"),
            Market.Create("La Bonne Cave", "Mauritius"),
        };

        await _dbContext.Markets.AddRangeAsync(markets, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded {Count} markets", markets.Length);
    }
}
