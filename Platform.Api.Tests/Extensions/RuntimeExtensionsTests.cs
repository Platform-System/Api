using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Platform.Api.Extensions;
using Xunit;

namespace Platform.Api.Tests.Extensions;

public sealed class RuntimeExtensionsTests
{
    [Fact]
    public async Task UsePlatformRuntime_WhenUnhandledExceptionOccurs_ReturnsProblemDetails()
    {
        await using var app = await CreateAppAsync(endpoints =>
        {
            endpoints.MapGet("/boom", (HttpContext _) => throw new InvalidOperationException("boom"));
        });

        var response = await app.GetTestClient().GetAsync("/boom");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status500InternalServerError, problem.Status);
        Assert.Equal("Internal server error", problem.Title);
        Assert.Equal("An unexpected error occurred.", problem.Detail);
        Assert.True(problem.Extensions.ContainsKey("traceId"));
    }

    [Fact]
    public async Task UsePlatformRuntime_MapsHealthEndpoint()
    {
        await using var app = await CreateAppAsync(_ => { });

        var response = await app.GetTestClient().GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<WebApplication> CreateAppAsync(Action<WebApplication> mapEndpoints)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddPlatformRuntime();

        var app = builder.Build();
        app.UsePlatformRuntime();
        mapEndpoints(app);
        await app.StartAsync();

        return app;
    }
}
