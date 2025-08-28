using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public class CookieToAuthorizationMiddleware
{
	private readonly RequestDelegate _next;

	public CookieToAuthorizationMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		var token = context.Request.Cookies["token"];
		if (!string.IsNullOrEmpty(token))
		{
			context.Request.Headers["Authorization"] = "Bearer " + token;
		}
		await _next(context);
	}
		
}