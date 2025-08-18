using api.Data;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseMySql(
		builder.Configuration.GetConnectionString("DatabaseConnection"),
		new MySqlServerVersion(new Version(8, 0, 43))
	));
builder.Services.AddSwaggerGen(options =>
	{
		options.SwaggerDoc(
			"v1",
			new OpenApiInfo
			{
				Title = "MJ SOLUTIONS API",
				Version = "1.0.0",
				Description = "MJ SOLUTIONS API, your solution for all needs.",
			});
	}
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
