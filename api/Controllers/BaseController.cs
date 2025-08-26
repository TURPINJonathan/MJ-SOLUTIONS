using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using api.Data;
using api.Services;
using AutoMapper;
using System.Security.Claims;

namespace api.Controllers
{
	public abstract class BaseController : ControllerBase
	{
		protected readonly AppDbContext _context;
		protected readonly ILogger _logger;
		protected readonly ILogger<FileService> _fileLogger;
		private readonly IMapper _mapper;
		protected readonly IConfiguration _configuration;

		protected BaseController(
			AppDbContext context,
			ILogger logger,
			ILogger<FileService> fileLogger,
			IMapper mapper,
			IConfiguration configuration
		)
		{
			_context = context;
			_logger = logger;
			_fileLogger = fileLogger;
			_mapper = mapper;
			_configuration = configuration;
		}

		protected string ConnectedUserEmail => User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity?.Name ?? "unknown";
		protected string ConnectedUserIp => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
		protected int? ConnectedUserId => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

	}
		
}