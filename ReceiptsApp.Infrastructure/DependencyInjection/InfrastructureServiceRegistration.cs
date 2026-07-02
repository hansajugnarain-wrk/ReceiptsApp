using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReceiptsApp.Application.Common.Interfaces;
using ReceiptsApp.Domain.Interfaces;
using ReceiptsApp.Infrastructure.Caching;
using ReceiptsApp.Infrastructure.Identity;
using ReceiptsApp.Infrastructure.Persistence;

namespace ReceiptsApp.Infrastructure.DependencyInjection;

/// <summary>
/// Wires up everything this project owns: the Users/Auth SQL Server store,
/// identity services (JWT, password hashing), the system clock, and the
/// distributed cache. Receipt persistence is registered separately by
/// ReceiptsApp.Infrastructure.Receipts, kept out of this assembly entirely.
/// </summary>
public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Users/Auth persistence — SQL Server, can point at the same Docker
        // container as Receipts or a fully separate instance.
        services.AddDbContext<UsersDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("UsersDatabase"),
                sql => sql.EnableRetryOnFailure(maxRetryCount: 3)));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        // Distributed cache — Redis in any environment where a connection
        // string is configured, falls back to in-memory for local dev
        // without Docker/Redis running.
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "ReceiptsApp:";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }
}
