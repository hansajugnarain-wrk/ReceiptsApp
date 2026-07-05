using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ReceiptsApp.Api.Extensions;
using ReceiptsApp.Api.Middleware;
using ReceiptsApp.Application.DependencyInjection;
using ReceiptsApp.Infrastructure.DependencyInjection;
using ReceiptsApp.Infrastructure.Logging;
using ReceiptsApp.Infrastructure.Persistence;
using ReceiptsApp.Infrastructure.Receipts.DependencyInjection;
using ReceiptsApp.Infrastructure.Receipts.Persistence;

var builder = WebApplication.CreateBuilder(args);

//Aspire configuration - disabled for Docker build
// builder.AddServiceDefaults();

// ── Logging ──────────────────────────────────────────────────────────────
// Replace the default console provider with log4net, configured by an
// external XML file so log levels/appenders can change without a rebuild.
builder.Logging.ClearProviders();
builder.Logging.AddLog4Net(Path.Combine(builder.Environment.ContentRootPath, "log4net.config"));
builder.Logging.SetMinimumLevel(LogLevel.Information);

// ── Layer registrations ─────────────────────────────────────────────────
// This Program.cs is the only place all four projects are referenced —
// the composition root. Application, Domain, and each Infrastructure
// project never reference one another except through interfaces.
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddReceiptsInfrastructure(builder.Configuration);

// ── Presentation ─────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddSwaggerWithJwt();
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<UsersDbContext>(name: "users-db")
    .AddDbContextCheck<ReceiptsDbContext>(name: "receipts-db")
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!, name: "redis");

var app = builder.Build();

// ── Database migration on startup (dev convenience) ───────────────────────
// In production, run `dotnet ef database update` per context as a deploy
// step instead of migrating automatically on boot.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var usersDb = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        await usersDb.Database.MigrateAsync();
    }
    catch (SqlException ex)
    {
        logger.LogError(ex, "Database migration failed - SQL connectivity issue. Connection string (no password): {Conn}",
            builder.Configuration.GetConnectionString("UsersDb"));
    }

    var receiptsDb = scope.ServiceProvider.GetRequiredService<ReceiptsDbContext>();
    await receiptsDb.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<ReceiptsDatabaseSeeder>();
    await seeder.SeedAsync();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Aspire configuration - disabled for Docker build
// app.MapDefaultEndpoints();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Exposed for WebApplicationFactory-based integration tests.
public partial class Program { }
