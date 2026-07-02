using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReceiptsApp.Domain.Entities;
using ReceiptsApp.Domain.Interfaces;
using ReceiptsApp.Infrastructure.Receipts.Persistence;

namespace ReceiptsApp.Infrastructure.Receipts.Repositories;

/// <summary>
/// Reads and writes Receipt aggregates against SQL Server (the Docker
/// container described in docker-compose.yml at the solution root, or any
/// SQL Server instance — the connection string is the only thing that
/// changes). Fully asynchronous: every method that touches the database
/// awaits an EF Core async API and accepts a CancellationToken.
/// </summary>
public class SqlReceiptRepository : IReceiptRepository
{
    private readonly ReceiptsDbContext _dbContext;
    private readonly ILogger<SqlReceiptRepository> _logger;

    public SqlReceiptRepository(ReceiptsDbContext dbContext, ILogger<SqlReceiptRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public Task<Receipt?> GetByIdAsync(
        Guid receiptId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching receipt {ReceiptId} for user {UserId}", receiptId, userId);

        return _dbContext.Receipts
            .Include(r => r.Market)
            .Include(r => r.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == receiptId && r.UserId == userId, cancellationToken);
    }

    public async Task<(IReadOnlyList<Receipt> Items, int TotalCount)> GetByUserAsync(
        Guid userId,
        Guid? marketId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Receipts
            .Include(r => r.Market)
            .AsNoTracking()
            .Where(r => r.UserId == userId);

        if (marketId.HasValue)
        {
            query = query.Where(r => r.MarketId == marketId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.PurchasedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        _logger.LogDebug(
            "Fetched {Count}/{Total} receipts for user {UserId} (page {Page})",
            items.Count, totalCount, userId, page);

        return (items, totalCount);
    }

    public async Task AddAsync(Receipt receipt, CancellationToken cancellationToken = default)
    {
        await _dbContext.Receipts.AddAsync(receipt, cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
