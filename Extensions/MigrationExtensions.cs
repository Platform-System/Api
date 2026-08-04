using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Api.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync<TContext>(this IApplicationBuilder app) where TContext : DbContext
    {
        using var scope = app.ApplicationServices.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var applyOnStartup = configuration.GetValue("Database:ApplyMigrationsOnStartup", true);

        if (!applyOnStartup)
        {
            return;
        }

        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        await context.Database.MigrateAsync();
    }
}
