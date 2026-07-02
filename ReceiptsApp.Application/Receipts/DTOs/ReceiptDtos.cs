namespace ReceiptsApp.Application.Receipts.DTOs;

public record ReceiptItemDto(string Name, decimal UnitPrice, int Quantity, decimal Subtotal);

public record ReceiptDto(
    Guid Id,
    string MarketName,
    string Description,
    decimal TotalAmount,
    string Currency,
    DateTime PurchasedAtUtc,
    IReadOnlyList<ReceiptItemDto> Items);

public record ReceiptSummaryDto(
    Guid Id,
    string MarketName,
    string Description,
    decimal TotalAmount,
    string Currency,
    DateTime PurchasedAtUtc);
