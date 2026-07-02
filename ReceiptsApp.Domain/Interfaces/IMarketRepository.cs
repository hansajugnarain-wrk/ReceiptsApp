using ReceiptsApp.Domain.Entities;

namespace ReceiptsApp.Domain.Interfaces;

public interface IMarketRepository
{
    Task<IReadOnlyList<Market>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Market?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
