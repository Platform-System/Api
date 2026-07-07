using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Platform.Api.Authorization;

public static class AuthorizationExtensions
{
    public static HttpContext? GetHttpContext(this AuthorizationHandlerContext context)
    {
        return context.Resource switch
        {
            HttpContext hc => hc,
            AuthorizationFilterContext afc => afc.HttpContext,
            _ => null
        };
    }
}
