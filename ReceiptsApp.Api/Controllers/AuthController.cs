using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReceiptsApp.Application.Auth.Commands;
using System.Security.Claims;

namespace ReceiptsApp.Api.Controllers;

/// <summary>
/// Thin HTTP boundary — every action maps a request to a MediatR command
/// and translates the Result back to an HTTP status. No business logic lives here.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequestModel request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RegisterCommand(request.Email, request.Password, request.DisplayName), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequestModel request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new LoginCommand(request.Email, request.Password), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : Unauthorized(new { error = result.Error });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException());

        await _sender.Send(new LogoutCommand(userId), cancellationToken);

        return NoContent();
    }
}

public record RegisterRequestModel(string Email, string Password, string DisplayName);

public record LoginRequestModel(string Email, string Password);
