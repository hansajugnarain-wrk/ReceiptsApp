using MediatR;
using Microsoft.Extensions.Logging;

namespace ReceiptsApp.Application.Common.Behaviors;

/// <summary>
/// Wraps every command/query with structured entry/exit/exception logging.
/// Because this is a MediatR pipeline behavior, individual handlers stay
/// focused purely on business orchestration — they never call a logger
/// themselves for "I started" / "I finished" noise.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("Handling {RequestName}", requestName);

        try
        {
            var response = await next();
            _logger.LogInformation("Handled {RequestName} successfully", requestName);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing {RequestName}", requestName);
            throw;
        }
    }
}
