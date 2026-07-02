using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReceiptsApp.Application.Receipts.Commands;
using ReceiptsApp.Application.Receipts.Queries;
using System.Security.Claims;

namespace ReceiptsApp.Api.Controllers;

[ApiController]
[Route("api/receipts")]
[Authorize]
public class ReceiptsController : ControllerBase
{
    private readonly ISender _sender;

    public ReceiptsController(ISender sender)
    {
        _sender = sender;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException());

    /// <summary>List receipts for the authenticated user — cache-first, optionally filtered by market.</summary>
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] Guid? marketId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetReceiptsQuery(CurrentUserId, marketId, page, pageSize), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    /// <summary>Get a single receipt by id, including line items.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetReceiptByIdQuery(id, CurrentUserId), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    /// <summary>Create a new receipt.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateReceiptRequestModel request, CancellationToken cancellationToken)
    {
        var command = new CreateReceiptCommand(
            CurrentUserId,
            request.MarketId,
            request.Description,
            request.Currency,
            request.PurchasedAtUtc,
            request.Items
                .Select(i => new CreateReceiptItemRequest(i.Name, i.UnitPrice, i.Quantity))
                .ToList());

        var result = await _sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }
}

public record CreateReceiptItemRequestModel(string Name, decimal UnitPrice, int Quantity);

public record CreateReceiptRequestModel(
    Guid MarketId,
    string Description,
    string Currency,
    DateTime PurchasedAtUtc,
    List<CreateReceiptItemRequestModel> Items);
