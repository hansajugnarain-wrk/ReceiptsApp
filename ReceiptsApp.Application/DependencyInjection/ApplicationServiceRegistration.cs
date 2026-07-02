using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ReceiptsApp.Application.Common.Behaviors;

namespace ReceiptsApp.Application.DependencyInjection;

public static class ApplicationServiceRegistration
{
    /// <summary>
    /// Registers MediatR handlers, FluentValidation validators, and the
    /// cross-cutting pipeline behaviors (validation runs before logging
    /// wraps the handler invocation) — all auto-discovered from this assembly.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = typeof(ApplicationServiceRegistration).Assembly;

        services.AddMediatR(config => config.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);

        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        return services;
    }
}
