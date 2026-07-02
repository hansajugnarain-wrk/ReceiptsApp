using log4net;
using log4net.Config;
using log4net.Repository;
using Microsoft.Extensions.Logging;

namespace ReceiptsApp.Infrastructure.Logging;

/// <summary>
/// Bridges Microsoft.Extensions.Logging (used everywhere via ILogger&lt;T&gt;
/// injection, including the LoggingBehavior pipeline) onto log4net, so the
/// actual sink/format/rolling-file configuration lives in one place
/// (log4net.config) instead of being hardcoded in C#.
/// </summary>
public sealed class Log4NetProvider : ILoggerProvider
{
    private readonly ILoggerRepository _repository;

    public Log4NetProvider(string configFilePath)
    {
        _repository = LogManager.CreateRepository(
            Guid.NewGuid().ToString(), typeof(log4net.Repository.Hierarchy.Hierarchy));

        XmlConfigurator.Configure(_repository, new FileInfo(configFilePath));
    }

    public ILogger CreateLogger(string categoryName) =>
        new Log4NetLogger(LogManager.GetLogger(_repository.Name, categoryName));

    public void Dispose() { }
}

internal sealed class Log4NetLogger : ILogger
{
    private readonly ILog _log;

    public Log4NetLogger(ILog log)
    {
        _log = log;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Trace or LogLevel.Debug => _log.IsDebugEnabled,
        LogLevel.Information => _log.IsInfoEnabled,
        LogLevel.Warning => _log.IsWarnEnabled,
        LogLevel.Error => _log.IsErrorEnabled,
        LogLevel.Critical => _log.IsFatalEnabled,
        _ => false
    };

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, exception);

        switch (logLevel)
        {
            case LogLevel.Trace:
            case LogLevel.Debug:
                _log.Debug(message, exception);
                break;
            case LogLevel.Information:
                _log.Info(message, exception);
                break;
            case LogLevel.Warning:
                _log.Warn(message, exception);
                break;
            case LogLevel.Error:
                _log.Error(message, exception);
                break;
            case LogLevel.Critical:
                _log.Fatal(message, exception);
                break;
        }
    }
}
