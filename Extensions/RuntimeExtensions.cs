using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Platform.Api.ErrorHandling;

namespace Platform.Api.Extensions;

public static class RuntimeExtensions
{
    public static IServiceCollection AddPlatformRuntime(this IServiceCollection services)
    {
        services.AddExceptionHandler<PlatformExceptionHandler>();
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            };
        });
        services.AddHealthChecks();

        return services;
    }

    public static WebApplication UsePlatformRuntime(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.MapHealthChecks("/health").AllowAnonymous();

        return app;
    }
}
