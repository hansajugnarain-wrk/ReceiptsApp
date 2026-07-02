namespace ReceiptsApp.Domain.Interfaces;

/// <summary>
/// Unit of work abstraction for the Users/Auth persistence store.
/// The Receipts bounded concern has its own SaveChangesAsync on
/// IReceiptRepository because it lives in a separate DbContext/database.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
