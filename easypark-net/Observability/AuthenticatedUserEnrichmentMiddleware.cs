using System.Security.Claims;
using Serilog.Context;

namespace EasyPark.Api.Observability;

public class AuthenticatedUserEnrichmentMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticatedUserEnrichmentMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? context.User.FindFirstValue("sub");
        var email = context.User.FindFirstValue(ClaimTypes.Email) ?? context.User.FindFirstValue("email");

        using (LogContext.PushProperty("AuthenticatedUserId", userId ?? "anonymous"))
        using (LogContext.PushProperty("AuthenticatedUserEmail", email ?? "anonymous"))
        {
            await _next(context);
        }
    }
}
