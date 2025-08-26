using Microsoft.AspNetCore.Mvc;
using api.Models;
using api.Data;
using api.Enums;
using api.DTOs;
using api.Helpers;
using BCrypt.Net;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly AppDbContext _context;
		private readonly IConfiguration _config;
		private readonly ILogger<AuthController> _logger;

		private static Dictionary<string, (int count, DateTime? blockedUntil)> loginAttempts = new();

		public AuthController(AppDbContext context, IConfiguration config, ILogger<AuthController> logger)
		{
			_context = context;
			_config = config;
			_logger = logger;
		}

		private string ConnectedUserEmail => User.Identity?.Name ?? "unknown";
		private string ConnectedUserIp => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

		[HttpPost("register")]
		[Authorize(Roles = "SUPER_ADMIN")]
		public async Task<IActionResult> Register([FromBody] RegisterDTO model)
		{
			if (!UserHelper.HasPermission(HttpContext, _context, "CREATE_USER"))
			{
				_logger.LogWarning($"Tentative de création d'utilisateur par {ConnectedUserEmail} depuis l'IP {ConnectedUserIp} sans permission.");
				AuditLogHelper.AddAudit(_context, "Échec création utilisateur (permission manquante)", ConnectedUserEmail, ConnectedUserIp, "User", null);
				await _context.SaveChangesAsync();
				return StatusCode(403, new { error = "Permission insuffisante." });
			}

			if (_context.Users.Any(u => u.Email == model.Email))
			{
				_logger.LogWarning($"Tentative de création d'utilisateur avec email déjà utilisé : {model.Email} par {ConnectedUserEmail} depuis l'IP {ConnectedUserIp}");
				AuditLogHelper.AddAudit(_context, $"Échec création utilisateur (email déjà utilisé : {model.Email})", ConnectedUserEmail, ConnectedUserIp, "User", null);
				await _context.SaveChangesAsync();
				return BadRequest(new { error = "Cet email est déjà utilisé." });
			}

			if (!PasswordHelper.IsPasswordValid(model.Password))
			{
				_logger.LogWarning($"Tentative de création d'utilisateur avec mot de passe non conforme par {ConnectedUserEmail} depuis l'IP {ConnectedUserIp}");
				AuditLogHelper.AddAudit(_context, "Échec création utilisateur (mot de passe non conforme)", ConnectedUserEmail, ConnectedUserIp, "User", null);
				await _context.SaveChangesAsync();
				return BadRequest(new { error = "Mot de passe non conforme." });
			}

			if (!ModelState.IsValid)
			{
				_logger.LogWarning($"Tentative de création d'utilisateur avec modèle invalide par {ConnectedUserEmail} depuis l'IP {ConnectedUserIp}");
				AuditLogHelper.AddAudit(_context, "Échec création utilisateur (modèle invalide)", ConnectedUserEmail, ConnectedUserIp, "User", null);
				await _context.SaveChangesAsync();
				return BadRequest(ModelState);
			}

			var user = new User
			{
				lastname = model.lastname,
				firstname = model.firstname,
				Email = model.Email,
				PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
				Role = model.Role ?? UserRoleEnum.USER,
				Permissions = new List<Permission>(),
				CreatedAt = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris")),
				UpdatedAt = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris"))
			};

			if (model.Permissions != null)
			{
				var permissions = _context.Permissions.Where(p => model.Permissions.Contains(p.Name)).ToList();
				user.Permissions = permissions;
			}

			_context.Users.Add(user);

			AuditLogHelper.AddAudit(_context, $"Utilisateur {user.Email} créé", ConnectedUserEmail, ConnectedUserIp, "User", user.Id);

			await _context.SaveChangesAsync();

			_logger.LogInformation($"Nouvel utilisateur créé : {user.Email} par {ConnectedUserEmail} depuis l'IP {ConnectedUserIp}");

			return Ok(new
			{
				Id = user.Id,
				Email = user.Email,
				Role = user.Role,
				Permissions = user.Permissions.Select(p => p.Name).ToList()
			});
		}

		[HttpPost("login")]
		public IActionResult Login([FromBody] LoginDTO login)
		{
			_logger.LogInformation($"Tentative de connexion pour {login.Email} depuis l'ip {ConnectedUserIp}");

			if (!loginAttempts.ContainsKey(ConnectedUserIp))
				loginAttempts[ConnectedUserIp] = (0, null);

			if (loginAttempts[ConnectedUserIp].blockedUntil.HasValue && loginAttempts[ConnectedUserIp].blockedUntil > TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris")))
			{
				_logger.LogWarning($"Connexion bloquée pour {login.Email} depuis {ConnectedUserIp} jusqu'à {loginAttempts[ConnectedUserIp].blockedUntil.Value.ToLocalTime()}");
				AuditLogHelper.AddAudit(_context, $"Blocage temporaire connexion pour {login.Email}", login.Email, ConnectedUserIp, "User", null);
				_context.SaveChanges();
				return StatusCode(429, $"Trop de tentatives. Réessayez après {loginAttempts[ConnectedUserIp].blockedUntil.Value.ToLocalTime()}.");
			}

			var user = _context.Users
					.Include(u => u.Permissions)
					.FirstOrDefault(u => u.Email == login.Email);

			if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
			{
				var (count, _) = loginAttempts[ConnectedUserIp];
				count++;
				DateTime? blockedUntil = null;
				_logger.LogWarning($"Échec de connexion pour {login.Email} depuis l'ip {ConnectedUserIp}. Nombre de tentatives: {count}");

				if (count >= 5)
				{
					_logger.LogWarning($"Blocage de l'ip {ConnectedUserIp} après 5 échecs de connexion.");
					blockedUntil = TimeZoneInfo.ConvertTime(DateTime.UtcNow.AddMinutes(10), TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris"));
				}

				loginAttempts[ConnectedUserIp] = (count, blockedUntil);

				AuditLogHelper.AddAudit(_context, $"Échec connexion pour {login.Email}", login.Email, ConnectedUserIp, "User", null);
				_context.SaveChanges();

				return Unauthorized(new { error = "Email ou mot de passe incorrect." });
			}

			loginAttempts[ConnectedUserIp] = (0, null);

			var claims = new List<Claim>
			{
					new Claim(ClaimTypes.Name, user.Email),
					new Claim(ClaimTypes.Role, user.Role?.ToString() ?? UserRoleEnum.USER.ToString()),
					new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
					new Claim("JwtVersion", user.JwtVersion.ToString())
			};

			foreach (var permission in user.Permissions)
			{
					claims.Add(new Claim("permissions", permission.Name));
			}

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
					issuer: _config["Jwt:Issuer"],
					claims: claims,
					expires: TimeZoneInfo.ConvertTime(DateTime.UtcNow.AddHours(1), TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris")),
					signingCredentials: creds);

			var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
			var expiryDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow.AddDays(7), TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris"));

			_context.RefreshTokens.Add(new RefreshToken
			{
				Token = refreshToken,
				UserId = user.Id,
				ExpiryDate = expiryDate
			});
			AuditLogHelper.AddAudit(_context, "Connexion réussie", login.Email, ConnectedUserIp, "User", user.Id);

			_context.SaveChanges();

			_logger.LogInformation($"Connexion réussie pour {login.Email} depuis {ConnectedUserIp}");

			return Ok(new
			{
				token = new JwtSecurityTokenHandler().WriteToken(token),
				refreshToken
			});
		}

		[HttpPost("refresh")]
		public IActionResult Refresh([FromBody] string refreshToken)
		{
			var tokenEntry = _context.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken && rt.ExpiryDate > TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris")));
			if (tokenEntry == null)
			{
				_logger.LogWarning($"Refresh token invalide ou expiré utilisé depuis l'IP {ConnectedUserIp}");
				AuditLogHelper.AddAudit(_context, "Échec refresh token (invalide ou expiré)", "unknown", ConnectedUserIp, "User", null);
				_context.SaveChanges();
				return Unauthorized(new { error = "Refresh token invalide ou expiré." });
			}

			var user = _context.Users.Find(tokenEntry.UserId);
			if (user == null)
			{
				_logger.LogWarning($"Refresh token utilisé pour un utilisateur non trouvé. Token: {refreshToken} depuis l'IP {ConnectedUserIp}");
				AuditLogHelper.AddAudit(_context, "Échec refresh token (utilisateur non trouvé)", "unknown", ConnectedUserIp, "User", null);
				_context.SaveChanges();
				return Unauthorized(new { error = "Utilisateur non trouvé." });
			}

			_context.RefreshTokens.Remove(tokenEntry);
			_context.SaveChanges();

			var newRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
			var expiryDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow.AddDays(7), TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris"));

			_context.RefreshTokens.Add(new RefreshToken
			{
				Token = newRefreshToken,
				UserId = user.Id,
				ExpiryDate = expiryDate
			});
			_context.SaveChanges();

			var claims = new[]
			{
								new Claim(ClaimTypes.Name, user.Email),
								new Claim(ClaimTypes.Role, user.Role?.ToString() ?? UserRoleEnum.USER.ToString())
						};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
					issuer: _config["Jwt:Issuer"],
					claims: claims,
					expires: TimeZoneInfo.ConvertTime(DateTime.UtcNow.AddHours(2), TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris")),
					signingCredentials: creds);

			_logger.LogInformation($"Refresh token utilisé pour {user.Email} depuis l'IP {ConnectedUserIp}");
			AuditLogHelper.AddAudit(_context, "Refresh token réussi", user.Email, ConnectedUserIp, "User", user.Id);
			_context.SaveChanges();

			return Ok(new
			{
				token = new JwtSecurityTokenHandler().WriteToken(token),
				refreshToken = newRefreshToken
			});
		}

		[HttpPost("logout")]
		[Authorize]
		public IActionResult Logout()
		{
			var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

			_context.BlacklistedTokens.Add(new BlacklistedToken
			{
				Token = token,
				BlacklistedAt = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris"))
			});
			AuditLogHelper.AddAudit(_context, "Déconnexion", ConnectedUserEmail, ConnectedUserIp, "User", null);

			_context.SaveChanges();

			_logger.LogInformation($"Déconnexion réussie pour {ConnectedUserEmail} depuis l'IP {ConnectedUserIp}");
			return Ok(new { message = "Déconnexion réussie." });
		}

		[HttpPatch("update")]
		[Authorize]
		public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDTO model)
		{
			var user = _context.Users.Include(u => u.Permissions).FirstOrDefault(u => u.Id == model.UserId);
			if (user == null)
			{
				_logger.LogWarning($"Modification échouée : utilisateur {model.UserId} non trouvé par {ConnectedUserEmail} depuis l'IP {ConnectedUserIp}");
				AuditLogHelper.AddAudit(_context, $"Échec modification utilisateur {model.UserId} (non trouvé)", ConnectedUserEmail, ConnectedUserIp, "User", null);
				await _context.SaveChangesAsync();
				return NotFound(new { error = "Utilisateur non trouvé." });
			}

			var connectedUser = _context.Users.FirstOrDefault(u => u.Email == ConnectedUserEmail);
			var isSuperAdmin = connectedUser?.Role == UserRoleEnum.SUPER_ADMIN;
			var isSelf = connectedUser?.Id == user.Id;

			if (!isSuperAdmin && !isSelf)
			{
				_logger.LogWarning($"Modification interdite : {ConnectedUserEmail} (id {connectedUser?.Id}) tente de modifier {user.Id} depuis l'IP {ConnectedUserIp}");
				AuditLogHelper.AddAudit(_context, $"Échec modification utilisateur {user.Id} (accès interdit)", ConnectedUserEmail, ConnectedUserIp, "User", user.Id);
				await _context.SaveChangesAsync();
				return StatusCode(403, new { error = "Accès interdit." });
			}

			if (model.Permissions != null && !isSuperAdmin)
			{
				_logger.LogWarning($"Modification des permissions interdite : {ConnectedUserEmail} tente de modifier les permissions de {user.Id} depuis l'IP {ConnectedUserIp}");
				AuditLogHelper.AddAudit(_context, $"Échec modification permissions utilisateur {user.Id} (non super admin)", ConnectedUserEmail, ConnectedUserIp, "User", user.Id);
				await _context.SaveChangesAsync();
				return StatusCode(403, new { error = "Seul le SUPER_ADMIN peut modifier les permissions." });
			}

			if (model.lastname != null)
				user.lastname = model.lastname;
			if (model.firstname != null)
				user.firstname = model.firstname;
			if (model.Email != null)
				user.Email = model.Email;
			if (model.Password != null)
				user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

			if (model.Permissions != null)
			{
				var permissions = _context.Permissions.Where(p => model.Permissions.Contains(p.Name)).ToList();
				user.Permissions = permissions;
			}

			user.UpdatedAt = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris"));

			_logger.LogInformation($"Modification réussie : utilisateur {user.Id} modifié par {ConnectedUserEmail} depuis l'IP {ConnectedUserIp}");
			AuditLogHelper.AddAudit(_context, $"Modification utilisateur {user.Id} réussie", ConnectedUserEmail, ConnectedUserIp, "User", user.Id);

			await _context.SaveChangesAsync();

			return Ok(new { user.Id, user.Email, user.Role, Permissions = user.Permissions.Select(p => p.Name).ToList() });
		}

	}
		
}