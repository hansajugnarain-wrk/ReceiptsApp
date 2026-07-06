using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReceiptsApp.Domain.Interfaces;
using ReceiptsApp.Infrastructure.Receipts.Caching;
using ReceiptsApp.Infrastructure.Receipts.Persistence;
using ReceiptsApp.Infrastructure.Receipts.Repositories;

namespace ReceiptsApp.Infrastructure.Receipts.DependencyInjection;

/// <summary>
/// Composition entry point for the Receipts infrastructure module. The API
/// project calls this in addition to AddInfrastructureServices — it is a
/// deliberately separate registration so the Receipts module can be
/// deployed, scaled, or replaced as an independent unit (e.g. moved behind
/// its own microservice later) without touching Auth/Users wiring.
/// </summary>
public static class ReceiptsInfrastructureServiceRegistration
{
    public static IServiceCollection AddReceiptsInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        

        // Register the concrete SQL repository first under its own type,
        // then wrap it with the caching decorator behind the interface that
        // Application actually consumes.
        services.AddScoped<SqlReceiptRepository>();
        services.AddScoped<IReceiptRepository>(provider =>
        {
            var sqlRepository = provider.GetRequiredService<SqlReceiptRepository>();
            var cacheService = provider.GetRequiredService<Application.Common.Interfaces.ICacheService>();
            var logger = provider.GetRequiredService<
                Microsoft.Extensions.Logging.ILogger<CachedReceiptRepository>>();

            return new CachedReceiptRepository(sqlRepository, cacheService, logger);
        });

        services.AddScoped<IMarketRepository, SqlMarketRepository>();
        services.AddScoped<ReceiptsDatabaseSeeder>();
        services.AddDbContextFactory<ReceiptsDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("ReceiptsDb")
            ));
        return services;
    }
}
