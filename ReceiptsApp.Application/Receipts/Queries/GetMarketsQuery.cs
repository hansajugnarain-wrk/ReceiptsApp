using MediatR;
using ReceiptsApp.Application.Common.Models;
using ReceiptsApp.Domain.Interfaces;

namespace ReceiptsApp.Application.Receipts.Queries;

public sealed record MarketDto(Guid Id, string Name, string Country);

/// <summary>Returns all available markets. No pagination — markets are a small, rarely-changing list.</summary>
public sealed record GetMarketsQuery : IRequest<Result<IReadOnlyList<MarketDto>>>;

public sealed class GetMarketsQueryHandler : IRequestHandler<GetMarketsQuery, Result<IReadOnlyList<MarketDto>>>
{
    private readonly IMarketRepository _marketRepository;

    public GetMarketsQueryHandler(IMarketRepository marketRepository)
    {
        _marketRepository = marketRepository;
    }

    public async Task<Result<IReadOnlyList<MarketDto>>> Handle(
        GetMarketsQuery request, CancellationToken cancellationToken)
    {
        var markets = await _marketRepository.GetAllAsync(cancellationToken);

        var dtos = markets
            .Select(m => new MarketDto(m.Id, m.Name, m.Country))
            .ToList();

        return Result.Ok<IReadOnlyList<MarketDto>>(dtos);
    }
}
