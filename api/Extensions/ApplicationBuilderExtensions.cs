using Microsoft.AspNetCore.Builder;

namespace api.Extensions
{
	public static class ApplicationBuilderExtensions
	{
		public static IApplicationBuilder UseCustomSecurityHeaders(this IApplicationBuilder app)
		{
			app.Use(async (context, next) =>
			{
				context.Response.Headers["X-Content-Type-Options"] = "nosniff";
				context.Response.Headers["X-Frame-Options"] = "DENY";
				context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
				context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
				await next();
			});
			return app;
		}

	}
		
}