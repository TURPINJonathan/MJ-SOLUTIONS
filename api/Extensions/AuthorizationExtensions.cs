using Microsoft.Extensions.DependencyInjection;

namespace api.Extensions
{
	public static class AuthorizationExtensions
	{
		public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
		{
			services.AddAuthorization(options =>
			{
				options.AddPolicy("CREATE_PROJECT", policy =>
									policy.RequireClaim("permissions", "CREATE_PROJECT"));
				options.AddPolicy("UPDATE_PROJECT", policy =>
									policy.RequireClaim("permissions", "UPDATE_PROJECT"));
				options.AddPolicy("DELETE_PROJECT", policy =>
									policy.RequireClaim("permissions", "DELETE_PROJECT"));
			});
			return services;
		}

	}
		
}