using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using api.Data;
using System.IdentityModel.Tokens.Jwt;

public class JwtVersionMiddleware
{
    private readonly RequestDelegate _next;

    public JwtVersionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (!string.IsNullOrEmpty(token))
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var email = jwt.Claims.FirstOrDefault(c => c.Type == "unique_name" || c.Type == "name")?.Value;
            var jwtVersionClaim = jwt.Claims.FirstOrDefault(c => c.Type == "jwtVersion")?.Value;

            if (email != null && jwtVersionClaim != null)
            {
                var user = db.Users.FirstOrDefault(u => u.Email == email);
                if (user != null && user.JwtVersion.ToString() != jwtVersionClaim)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Token révoqué.");
                    return;
                }
            }
        }
        await _next(context);
    }
}