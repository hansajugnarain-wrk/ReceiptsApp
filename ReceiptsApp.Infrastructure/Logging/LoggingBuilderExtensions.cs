using Microsoft.Extensions.Logging;

namespace ReceiptsApp.Infrastructure.Logging;

public static class LoggingBuilderExtensions
{
    public static ILoggingBuilder AddLog4Net(this ILoggingBuilder builder, string configFilePath)
    {
        builder.AddProvider(new Log4NetProvider(configFilePath));
        return builder;
    }
}
