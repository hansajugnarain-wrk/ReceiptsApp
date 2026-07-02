using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReceiptsApp.Application.Receipts.Queries;

namespace ReceiptsApp.Api.Controllers;

[ApiController]
[Route("api/markets")]
[Authorize]
public class MarketsController : ControllerBase
{
    private readonly ISender _sender;

    public MarketsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>List all available markets. Results come from the Application layer via MediatR.</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMarketsQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
}
