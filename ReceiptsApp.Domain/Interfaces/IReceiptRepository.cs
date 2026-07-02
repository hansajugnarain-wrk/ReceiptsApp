using ReceiptsApp.Domain.Entities;

namespace ReceiptsApp.Domain.Interfaces;

/// <summary>
/// Persistence contract for Receipt. Implemented in the dedicated
/// ReceiptsApp.Infrastructure.Receipts project, kept separate from the
/// rest of Infrastructure to isolate the receipts bounded concern
/// (its own DbContext, its own SQL Server instance, its own caching).
/// </summary>
public interface IReceiptRepository
{
    Task<Receipt?> GetByIdAsync(Guid receiptId, Guid userId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Receipt> Items, int TotalCount)> GetByUserAsync(
        Guid userId,
        Guid? marketId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(Receipt receipt, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
