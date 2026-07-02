using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using ReceiptsApp.Application.Common.Interfaces;
using ReceiptsApp.Application.Common.Models;
using ReceiptsApp.Domain.Entities;
using ReceiptsApp.Domain.Interfaces;

namespace ReceiptsApp.Application.Receipts.Commands;

public sealed record CreateReceiptItemRequest(string Name, decimal UnitPrice, int Quantity);

public sealed record CreateReceiptCommand(
    Guid UserId,
    Guid MarketId,
    string Description,
    string Currency,
    DateTime PurchasedAtUtc,
    IReadOnlyList<CreateReceiptItemRequest> Items) : IRequest<Result<Guid>>;

public sealed class CreateReceiptCommandValidator : AbstractValidator<CreateReceiptCommand>
{
    public CreateReceiptCommandValidator()
    {
        RuleFor(x => x.MarketId).NotEmpty();
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Items).NotEmpty().WithMessage("A receipt must have at least one item.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Name).NotEmpty();
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}

/// <summary>
/// Creates a receipt and invalidates every cached receipt-list page for the
/// user so the next GetReceiptsQuery reflects the new entry instead of
/// serving a stale page out of the cache.
/// </summary>
public sealed class CreateReceiptCommandHandler : IRequestHandler<CreateReceiptCommand, Result<Guid>>
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IMarketRepository _marketRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CreateReceiptCommandHandler> _logger;

    public CreateReceiptCommandHandler(
        IReceiptRepository receiptRepository,
        IMarketRepository marketRepository,
        ICacheService cacheService,
        ILogger<CreateReceiptCommandHandler> logger)
    {
        _receiptRepository = receiptRepository;
        _marketRepository = marketRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateReceiptCommand request, CancellationToken cancellationToken)
    {
        var market = await _marketRepository.GetByIdAsync(request.MarketId, cancellationToken);
        if (market is null)
        {
            return Result.Fail<Guid>("Market not found.");
        }

        var items = request.Items
            .Select(i => ReceiptItem.Create(i.Name, i.UnitPrice, i.Quantity))
            .ToList();

        var receipt = Receipt.Create(
            request.UserId, request.MarketId, request.Description,
            request.Currency, request.PurchasedAtUtc, items);

        await _receiptRepository.AddAsync(receipt, cancellationToken);
        await _receiptRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync($"user:{request.UserId}:receipts", cancellationToken);

        _logger.LogInformation("Receipt {ReceiptId} created for user {UserId}", receipt.Id, request.UserId);

        return Result.Ok(receipt.Id);
    }
}
