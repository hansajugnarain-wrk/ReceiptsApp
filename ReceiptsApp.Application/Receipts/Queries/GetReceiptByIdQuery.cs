using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using ReceiptsApp.Application.Common.Interfaces;
using ReceiptsApp.Application.Common.Models;
using ReceiptsApp.Application.Receipts.DTOs;
using ReceiptsApp.Domain.Interfaces;

namespace ReceiptsApp.Application.Receipts.Queries;

public sealed record GetReceiptByIdQuery(Guid ReceiptId, Guid UserId) : IRequest<Result<ReceiptDto>>;

public sealed class GetReceiptByIdQueryValidator : AbstractValidator<GetReceiptByIdQuery>
{
    public GetReceiptByIdQueryValidator()
    {
        RuleFor(x => x.ReceiptId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public sealed class GetReceiptByIdQueryHandler : IRequestHandler<GetReceiptByIdQuery, Result<ReceiptDto>>
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetReceiptByIdQueryHandler> _logger;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public GetReceiptByIdQueryHandler(
        IReceiptRepository receiptRepository,
        ICacheService cacheService,
        ILogger<GetReceiptByIdQueryHandler> logger)
    {
        _receiptRepository = receiptRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<ReceiptDto>> Handle(GetReceiptByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"user:{request.UserId}:receipt:{request.ReceiptId}";

        var cached = await _cacheService.GetAsync<ReceiptDto>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogInformation("Cache hit for {CacheKey}", cacheKey);
            return Result.Ok(cached);
        }

        var receipt = await _receiptRepository.GetByIdAsync(request.ReceiptId, request.UserId, cancellationToken);
        if (receipt is null)
        {
            return Result.Fail<ReceiptDto>("Receipt not found.");
        }

        var dto = new ReceiptDto(
            receipt.Id,
            receipt.Market.Name,
            receipt.Description,
            receipt.TotalAmount,
            receipt.Currency,
            receipt.PurchasedAtUtc,
            receipt.Items.Select(i => new ReceiptItemDto(i.Name, i.UnitPrice, i.Quantity, i.Subtotal)).ToList());

        await _cacheService.SetAsync(cacheKey, dto, CacheDuration, cancellationToken);

        return Result.Ok(dto);
    }
}
