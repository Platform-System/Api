using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Api.ErrorHandling;

namespace Api.Extensions;

public static class RuntimeExtensions
{
    public static IServiceCollection AddRuntime(this IServiceCollection services)
    {
        services.AddExceptionHandler<ApiExceptionHandler>();
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

    public static WebApplication UseRuntime(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.MapHealthChecks("/health").AllowAnonymous();

        return app;
    }
}
