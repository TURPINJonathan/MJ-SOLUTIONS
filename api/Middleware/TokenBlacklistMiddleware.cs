using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using api.Data;

public class TokenBlacklistMiddleware
{
    private readonly RequestDelegate _next;

    public TokenBlacklistMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (!string.IsNullOrEmpty(token) && db.BlacklistedTokens.Any(t => t.Token == token))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Token blacklisted.");
            return;
        }
        await _next(context);
    }
}