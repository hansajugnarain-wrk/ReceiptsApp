using Microsoft.EntityFrameworkCore;
using ReceiptsApp.Domain.Entities;
using ReceiptsApp.Domain.Interfaces;
using ReceiptsApp.Infrastructure.Receipts.Persistence;

namespace ReceiptsApp.Infrastructure.Receipts.Repositories;

public class SqlMarketRepository : IMarketRepository
{
    private readonly ReceiptsDbContext _dbContext;

    public SqlMarketRepository(ReceiptsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<IReadOnlyList<Market>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _dbContext.Markets
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<Market>)t.Result, cancellationToken);

    public Task<Market?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Markets.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
}
