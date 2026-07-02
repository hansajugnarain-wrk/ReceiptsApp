using Microsoft.Extensions.Logging;
using ReceiptsApp.Application.Common.Interfaces;
using ReceiptsApp.Domain.Entities;
using ReceiptsApp.Domain.Interfaces;

namespace ReceiptsApp.Infrastructure.Receipts.Caching;

/// <summary>
/// Decorator over IReceiptRepository that caches single-receipt lookups at
/// the repository level. This sits underneath the Application-layer caching
/// already done in GetReceiptByIdQueryHandler — defense in depth is shown
/// here as a teaching example of the decorator pattern, not because both
/// layers are required in every system. Writes go straight through and
/// also evict this repository's own cache entry so it can never serve a
/// stale receipt after CreateReceiptCommandHandler runs.
/// </summary>
public class CachedReceiptRepository : IReceiptRepository
{
    private readonly IReceiptRepository _inner;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachedReceiptRepository> _logger;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public CachedReceiptRepository(
        IReceiptRepository inner,
        ICacheService cacheService,
        ILogger<CachedReceiptRepository> logger)
    {
        _inner = inner;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Receipt?> GetByIdAsync(
        Guid receiptId, Guid userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildSingleReceiptKey(receiptId, userId);

        var cached = await _cacheService.GetAsync<Receipt>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogDebug("Repository-level cache hit for {CacheKey}", cacheKey);
            return cached;
        }

        var receipt = await _inner.GetByIdAsync(receiptId, userId, cancellationToken);
        if (receipt is not null)
        {
            await _cacheService.SetAsync(cacheKey, receipt, CacheDuration, cancellationToken);
        }

        return receipt;
    }

    public Task<(IReadOnlyList<Receipt> Items, int TotalCount)> GetByUserAsync(
        Guid userId, Guid? marketId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        // List queries are already cached at the Application layer with
        // richer key composition (filters, pagination); pass through here.
        _inner.GetByUserAsync(userId, marketId, page, pageSize, cancellationToken);

    public Task AddAsync(Receipt receipt, CancellationToken cancellationToken = default) =>
        _inner.AddAsync(receipt, cancellationToken);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await _inner.SaveChangesAsync(cancellationToken);
        return result;
    }

    private static string BuildSingleReceiptKey(Guid receiptId, Guid userId) =>
        $"user:{userId}:receipt:{receiptId}:repo-cache";
}
