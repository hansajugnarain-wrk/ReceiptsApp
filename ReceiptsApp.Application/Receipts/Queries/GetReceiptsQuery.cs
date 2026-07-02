using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using ReceiptsApp.Application.Common.Interfaces;
using ReceiptsApp.Application.Common.Models;
using ReceiptsApp.Application.Receipts.DTOs;
using ReceiptsApp.Domain.Interfaces;

namespace ReceiptsApp.Application.Receipts.Queries;

public sealed record GetReceiptsQuery(
    Guid UserId,
    Guid? MarketId,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PagedResult<ReceiptSummaryDto>>>;

public sealed class GetReceiptsQueryValidator : AbstractValidator<GetReceiptsQuery>
{
    public GetReceiptsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

/// <summary>
/// Lists receipts for a user, cache-first. The cache key incorporates every
/// parameter that affects the result so different filters/pages never collide.
/// LogoutCommandHandler evicts everything under "user:{id}" on logout, and
/// CreateReceiptCommandHandler evicts it on write — keeping reads fast without
/// risking stale data after a mutation.
/// </summary>
public sealed class GetReceiptsQueryHandler
    : IRequestHandler<GetReceiptsQuery, Result<PagedResult<ReceiptSummaryDto>>>
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetReceiptsQueryHandler> _logger;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public GetReceiptsQueryHandler(
        IReceiptRepository receiptRepository,
        ICacheService cacheService,
        ILogger<GetReceiptsQueryHandler> logger)
    {
        _receiptRepository = receiptRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<PagedResult<ReceiptSummaryDto>>> Handle(
        GetReceiptsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = BuildCacheKey(request);

        var pagedResult = await _cacheService.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                _logger.LogInformation(
                    "Cache miss for {CacheKey}; querying receipt store", cacheKey);

                var (items, total) = await _receiptRepository.GetByUserAsync(
                    request.UserId, request.MarketId, request.Page, request.PageSize, ct);

                var dtos = items
                    .Select(r => new ReceiptSummaryDto(
                        r.Id, r.Market.Name, r.Description, r.TotalAmount, r.Currency, r.PurchasedAtUtc))
                    .ToList();

                return new PagedResult<ReceiptSummaryDto>(dtos, total, request.Page, request.PageSize);
            },
            CacheDuration,
            cancellationToken);

        return Result.Ok(pagedResult);
    }

    private static string BuildCacheKey(GetReceiptsQuery request) =>
        $"user:{request.UserId}:receipts:market:{request.MarketId?.ToString() ?? "all"}:page:{request.Page}:size:{request.PageSize}";
}
