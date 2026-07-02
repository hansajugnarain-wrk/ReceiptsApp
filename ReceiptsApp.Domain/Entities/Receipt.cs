using ReceiptsApp.Domain.Exceptions;

namespace ReceiptsApp.Domain.Entities;

/// <summary>
/// Aggregate root for a purchase receipt. Owns its ReceiptItem collection —
/// items cannot be added or removed outside of Receipt's own methods,
/// which keeps TotalAmount always consistent with the line items.
/// </summary>
public class Receipt : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid MarketId { get; private set; }
    public Market Market { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; } = default!;
    public DateTime PurchasedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private readonly List<ReceiptItem> _items = new();
    public IReadOnlyCollection<ReceiptItem> Items => _items.AsReadOnly();

    private Receipt() { }

    public static Receipt Create(
        Guid userId,
        Guid marketId,
        string description,
        string currency,
        DateTime purchasedAtUtc,
        IReadOnlyCollection<ReceiptItem> items)
    {
        if (items is null || items.Count == 0)
            throw new DomainValidationException("A receipt must contain at least one item.");

        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
            throw new DomainValidationException("Currency must be a 3-letter ISO code.");

        var receipt = new Receipt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MarketId = marketId,
            Description = description?.Trim() ?? string.Empty,
            Currency = currency.Trim().ToUpperInvariant(),
            PurchasedAtUtc = purchasedAtUtc,
            CreatedAtUtc = DateTime.UtcNow
        };

        receipt._items.AddRange(items);
        receipt.TotalAmount = receipt._items.Sum(i => i.Subtotal);

        return receipt;
    }
}

/// <summary>A single line item belonging to a Receipt. Never created standalone.</summary>
public class ReceiptItem : BaseEntity
{
    public Guid ReceiptId { get; private set; }
    public string Name { get; private set; } = default!;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal Subtotal => UnitPrice * Quantity;

    private ReceiptItem() { }

    public static ReceiptItem Create(string name, decimal unitPrice, int quantity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Item name is required.");
        if (unitPrice < 0)
            throw new DomainValidationException("Unit price cannot be negative.");
        if (quantity <= 0)
            throw new DomainValidationException("Quantity must be positive.");

        return new ReceiptItem
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            UnitPrice = unitPrice,
            Quantity = quantity
        };
    }
}
