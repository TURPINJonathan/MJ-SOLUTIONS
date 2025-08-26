using api.Data;
using api.Extensions;
using api.Mapper;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
if (allowedOrigins == null || allowedOrigins.Length == 0)
    allowedOrigins = new[] { "*" };

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DatabaseConnection"),
        new MySqlServerVersion(new Version(8, 0, 43))
    ));
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddCustomAuthorization();

builder.Services.AddCustomSwagger();
builder.Services.AddCustomAuthentication(builder.Configuration);
builder.Services.AddCustomCors(allowedOrigins);

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/log.txt")
    .CreateLogger();
builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseCustomSecurityHeaders();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<TokenBlacklistMiddleware>();
app.UseMiddleware<JwtVersionMiddleware>();

app.MapControllers();

app.Run();