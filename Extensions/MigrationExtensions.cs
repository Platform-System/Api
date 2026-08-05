using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Api.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync<TContext>(this IApplicationBuilder app) where TContext : DbContext
    {
        using var scope = app.ApplicationServices.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(MigrationExtensions));
        var applyOnStartup = configuration.GetValue("Database:ApplyMigrationsOnStartup", true);

        if (!applyOnStartup)
        {
            logger.LogInformation("Skipping database migrations for {DbContext} because Database:ApplyMigrationsOnStartup is false.", typeof(TContext).Name);
            return;
        }

        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToArray();

        if (pendingMigrations.Length == 0)
        {
            logger.LogInformation("No pending migrations found for {DbContext}.", typeof(TContext).Name);
            return;
        }

        logger.LogInformation(
            "Applying {MigrationCount} pending migrations for {DbContext}: {Migrations}",
            pendingMigrations.Length,
            typeof(TContext).Name,
            string.Join(", ", pendingMigrations));

        await context.Database.MigrateAsync();

        logger.LogInformation("Database migrations applied successfully for {DbContext}.", typeof(TContext).Name);
    }
}
