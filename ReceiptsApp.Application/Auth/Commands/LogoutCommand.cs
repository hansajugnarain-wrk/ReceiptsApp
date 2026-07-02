using MediatR;
using Microsoft.Extensions.Logging;
using ReceiptsApp.Application.Common.Interfaces;
using ReceiptsApp.Application.Common.Models;

namespace ReceiptsApp.Application.Auth.Commands;

/// <summary>
/// With stateless JWTs, "logout" mainly means evicting any cached session
/// data server-side. Real token revocation requires a refresh-token store
/// (e.g. a database table) checked on every refresh — left as an extension point.
/// </summary>
public sealed record LogoutCommand(Guid UserId) : IRequest<Result<bool>>;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<bool>>
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(ICacheService cacheService, ILogger<LogoutCommandHandler> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await _cacheService.RemoveByPrefixAsync($"user:{request.UserId}", cancellationToken);

        _logger.LogInformation("User {UserId} logged out, cache evicted", request.UserId);

        return Result.Ok(true);
    }
}
