using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace api.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
		{
			services.AddEndpointsApiExplorer();
			services.AddSwaggerGen(options =>
			{
				options.SwaggerDoc(
									"v1",
									new OpenApiInfo
									{
										Title = "MJ SOLUTIONS API",
										Version = "1.0.0",
										Description = "MJ SOLUTIONS API, your solution for all needs.",
									});

				options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
				{
					In = ParameterLocation.Header,
					Description = "JWT Authorization header using the Bearer scheme.",
					Name = "Authorization",
					Type = SecuritySchemeType.ApiKey
				});

				options.AddSecurityRequirement(new OpenApiSecurityRequirement
					{
						{
								new OpenApiSecurityScheme
								{
										Reference = new OpenApiReference
										{
												Type = ReferenceType.SecurityScheme,
												Id = "Bearer"
										}
								},
								new string[] {}
						}
					});
			});
			return services;
		}

		public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration config)
		{
			var jwtKey = config.GetValue<string>("Jwt:Key") ?? throw new ArgumentNullException("Jwt:Key");
			var jwtIssuer = config.GetValue<string>("Jwt:Issuer") ?? throw new ArgumentNullException("Jwt:Issuer");

			services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = false,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = jwtIssuer,
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
				};
			});

			services.AddAuthorization();
			return services;
		}

		public static IServiceCollection AddCustomCors(this IServiceCollection services, string[] allowedOrigins)
		{
			services.AddCors(options =>
			{
				options.AddPolicy("AllowFrontend", policy =>
							{
								policy.WithOrigins(allowedOrigins)
														.AllowAnyHeader()
														.AllowAnyMethod();
							});
			});
			return services;
		}

	}
		
}