using Microsoft.AspNetCore.Mvc;
using api.Models;
using api.Data;
using api.Enums;
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

		// Protection brute force : IP -> (tentatives, blocage jusqu'à)
		private static Dictionary<string, (int count, DateTime? blockedUntil)> loginAttempts = new();

		public AuthController(AppDbContext context, IConfiguration config, ILogger<AuthController> logger)
		{
			_context = context;
			_config = config;
			_logger = logger;
		}

		[HttpPost("register")]
		[Authorize(Roles = "SUPER_ADMIN")]
		public async Task<IActionResult> Register([FromBody] RegisterModel model)
		{
			var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
			var userConnected = User.Identity?.Name ?? "unknown";

			if (!UserHelper.HasPermission(HttpContext, _context, "CREATE_USER"))
			{
				_logger.LogWarning($"Tentative de création d'utilisateur par {userConnected} depuis l'IP {ip} sans permission.");
				_context.AuditLogs.Add(new AuditLog
				{
					Action = "Échec création utilisateur (permission manquante)",
					UserEmail = userConnected,
					Date = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris")),
					IpAddress = ip
				});
				await _context.SaveChangesAsync();
				return StatusCode(403, new { error = "Permission insuffisante." });
			}

			if (_context.Users.Any(u => u.Email == model.Email))
			{
				_logger.LogWarning($"Tentative de création d'utilisateur avec email déjà utilisé : {model.Email} par {userConnected} depuis l'IP {ip}");
				_context.AuditLogs.Add(new AuditLog
				{
					Action = $"Échec création utilisateur (email déjà utilisé : {model.Email})",
					UserEmail = userConnected,
					Date = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris")),
					IpAddress = ip
				});
				await _context.SaveChangesAsync();
				return BadRequest(new { error = "Cet email est déjà utilisé." });
			}

			// Validation du mot de passe
			if (string.IsNullOrWhiteSpace(model.Password) ||
					model.Password.Length < 8 ||
					!model.Password.Any(char.IsUpper) ||
					!model.Password.Any(char.IsLower) ||
					!model.Password.Any(char.IsDigit) ||
					!model.Password.Any(ch => !char.IsLetterOrDigit(ch)))
			{
				_logger.LogWarning($"Tentative de création d'utilisateur avec mot de passe non conforme par {userConnected} depuis l'IP {ip}");
				_context.AuditLogs.Add(new AuditLog
				{
					Action = "Échec création utilisateur (mot de passe non conforme)",
					UserEmail = userConnected,
					Date = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris")),
					IpAddress = ip
				});
				await _context.SaveChangesAsync();
				return BadRequest(new { error = "Mot de passe non conforme." });
			}

			if (!ModelState.IsValid)
			{
				_logger.LogWarning($"Tentative de création d'utilisateur avec modèle invalide par {userConnected} depuis l'IP {ip}");
				_context.AuditLogs.Add(new AuditLog
				{
					Action = "Échec création utilisateur (modèle invalide)",
					UserEmail = userConnected,
					Date = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris")),
					IpAddress = ip
				});
				await _context.SaveChangesAsync();
				return BadRequest(ModelState);
			}

			var user = new User
			{
				lastname = model.lastname,
				firstname = model.firstname,
				Email = model.Email,
				PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
				Role = model.Role ?? UserRole.USER,
				Permissions = new List<Permission>(),
				CreatedAt = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris")),
				UpdatedAt = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris"))
			};

			// Attribution des permissions
			if (model.Permissions != null)
			{
				var permissions = _context.Permissions.Where(p => model.Permissions.Contains(p.Name)).ToList();
				user.Permissions = permissions;
			}

			_context.Users.Add(user);

			_context.AuditLogs.Add(new AuditLog
			{
				Action = $"Utilisateur {user.Email} créé",
				UserEmail = userConnected,
				Date = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris")),
				IpAddress = ip
			});

			await _context.SaveChangesAsync();

			_logger.LogInformation($"Nouvel utilisateur créé : {user.Email} par {userConnected} depuis l'IP {ip}");

			return Ok(new
			{
				Id = user.Id,
				Email = user.Email,
				Role = user.Role,
				Permissions = user.Permissions.Select(p => p.Name).ToList()
			});
		}

		[HttpPost("login")]
		public IActionResult Login([FromBody] LoginModel login)
		{
			var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
			_logger.LogInformation($"Tentative de connexion pour {login.Email} depuis l'ip {ip}");

			if (!loginAttempts.ContainsKey(ip))
				loginAttempts[ip] = (0, null);

			// Blocage temporaire après 5 tentatives
			if (loginAttempts[ip].blockedUntil.HasValue && loginAttempts[ip].blockedUntil > TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris")))
			{
				_logger.LogWarning($"Connexion bloquée pour {login.Email} depuis {ip} jusqu'à {loginAttempts[ip].blockedUntil.Value.ToLocalTime()}");
				_context.AuditLogs.Add(new AuditLog
				{
					Action = $"Blocage temporaire connexion pour {login.Email}",
					UserEmail = login.Email,
					Date = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris")),
					IpAddress = ip
				});
				_context.SaveChanges();
				return StatusCode(429, $"Trop de tentatives. Réessayez après {loginAttempts[ip].blockedUntil.Value.ToLocalTime()}.");
			}

			var user = _context.Users.FirstOrDefault(u => u.Email == login.Email);

			if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
			{
				var (count, _) = loginAttempts[ip];
				count++;
				DateTime? blockedUntil = null;
				_logger.LogWarning($"Échec de connexion pour {login.Email} depuis l'ip {ip}. Nombre de tentatives: {count}");

				if (count >= 5)
				{
					_logger.LogWarning($"Blocage de l'ip {ip} après 5 échecs de connexion.");
					blockedUntil = TimeZoneInfo.ConvertTime(DateTime.UtcNow.AddMinutes(10), TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris"));
				}

				loginAttempts[ip] = (count, blockedUntil);

				_context.AuditLogs.Add(new AuditLog
				{
					Action = $"Échec connexion pour {login.Email}",
					UserEmail = login.Email,
					Date = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris")),
					IpAddress = ip
				});
				_context.SaveChanges();

				return Unauthorized(new { error = "Email ou mot de passe incorrect." });
			}

			// Si login réussi, reset le compteur
			loginAttempts[ip] = (0, null);

			var claims = new[]
			{
								new Claim(ClaimTypes.Name, user.Email),
								new Claim(ClaimTypes.Role, user.Role?.ToString() ?? UserRole.USER.ToString()),
								new Claim("JwtVersion", user.JwtVersion.ToString())
						};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
					issuer: _config["Jwt:Issuer"],
					claims: claims,
					expires: TimeZoneInfo.ConvertTime(DateTime.UtcNow.AddHours(1), TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris")),
					signingCredentials: creds);

			// Génération du refresh token
			var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
			var expiryDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow.AddDays(7), TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris"));

			_context.RefreshTokens.Add(new RefreshToken
			{
				Token = refreshToken,
				UserId = user.Id,
				ExpiryDate = expiryDate
			});
			_context.AuditLogs.Add(new AuditLog
			{
				Action = "Connexion réussie",
				UserEmail = login.Email,
				Date = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris")),
				IpAddress = ip
			});

			_context.SaveChanges();

			_logger.LogInformation($"Connexion réussie pour {login.Email} depuis {ip}");

			return Ok(new
			{
				token = new JwtSecurityTokenHandler().WriteToken(token),
				refreshToken
			});
		}

		[HttpPost("refresh")]
		public IActionResult Refresh([FromBody] string refreshToken)
		{
			var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
			var tokenEntry = _context.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken && rt.ExpiryDate > TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris")));
			if (tokenEntry == null)
			{
				_logger.LogWarning($"Refresh token invalide ou expiré utilisé depuis l'IP {ip}");
				_context.AuditLogs.Add(new AuditLog
				{
					Action = "Échec refresh token (invalide ou expiré)",
					UserEmail = "unknown",
					Date = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris")),
					IpAddress = ip
				});
				_context.SaveChanges();
				return Unauthorized(new { error = "Refresh token invalide ou expiré." });
			}

			var user = _context.Users.Find(tokenEntry.UserId);
			if (user == null)
			{
				_logger.LogWarning($"Refresh token utilisé pour un utilisateur non trouvé. Token: {refreshToken} depuis l'IP {ip}");
				_context.AuditLogs.Add(new AuditLog
				{
					Action = "Échec refresh token (utilisateur non trouvé)",
					UserEmail = "unknown",
					Date = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris")),
					IpAddress = ip
				});
				_context.SaveChanges();
				return Unauthorized(new { error = "Utilisateur non trouvé." });
			}

			// Supprime le refresh token utilisé
			_context.RefreshTokens.Remove(tokenEntry);
			_context.SaveChanges();

			// Génère un nouveau refresh token
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
								new Claim(ClaimTypes.Role, user.Role?.ToString() ?? UserRole.USER.ToString())
						};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
					issuer: _config["Jwt:Issuer"],
					claims: claims,
					expires: TimeZoneInfo.ConvertTime(DateTime.UtcNow.AddHours(2), TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris")),
					signingCredentials: creds);

			_logger.LogInformation($"Refresh token utilisé pour {user.Email} depuis l'IP {ip}");
			_context.AuditLogs.Add(new AuditLog
			{
				Action = "Refresh token réussi",
				UserEmail = user.Email,
				Date = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris")),
				IpAddress = ip
			});
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
			var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
			var userConnected = User.Identity?.Name ?? "unknown";
			var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

			_context.BlacklistedTokens.Add(new BlacklistedToken
			{
				Token = token,
				BlacklistedAt = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris"))
			});
			_context.AuditLogs.Add(new AuditLog
			{
				Action = "Déconnexion",
				UserEmail = userConnected,
				Date = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris")),
				IpAddress = ip
			});

			_context.SaveChanges();

			_logger.LogInformation($"Déconnexion réussie pour {userConnected} depuis l'IP {ip}");
			return Ok(new { message = "Déconnexion réussie." });
		}

		[HttpPatch("update")]
		[Authorize]
		public async Task<IActionResult> UpdateUser([FromBody] UpdateUserModel model)
		{
			var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
			var userConnected = User.Identity?.Name ?? "unknown";

			var user = _context.Users.Include(u => u.Permissions).FirstOrDefault(u => u.Id == model.UserId);
			if (user == null)
			{
				_logger.LogWarning($"Modification échouée : utilisateur {model.UserId} non trouvé par {userConnected} depuis l'IP {ip}");
				_context.AuditLogs.Add(new AuditLog
				{
					Action = $"Échec modification utilisateur {model.UserId} (non trouvé)",
					UserEmail = userConnected,
					Date = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris")),
					IpAddress = ip
				});
				await _context.SaveChangesAsync();
				return NotFound(new { error = "Utilisateur non trouvé." });
			}

			var connectedUser = _context.Users.FirstOrDefault(u => u.Email == userConnected);
			var isSuperAdmin = connectedUser?.Role == UserRole.SUPER_ADMIN;
			var isSelf = connectedUser?.Id == user.Id;

			if (!isSuperAdmin && !isSelf)
			{
				_logger.LogWarning($"Modification interdite : {userConnected} (id {connectedUser?.Id}) tente de modifier {user.Id} depuis l'IP {ip}");
				_context.AuditLogs.Add(new AuditLog
				{
					Action = $"Échec modification utilisateur {user.Id} (accès interdit)",
					UserEmail = userConnected,
					Date = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris")),
					IpAddress = ip
				});
				await _context.SaveChangesAsync();
				return StatusCode(403, new { error = "Accès interdit." });
			}

			// Seul le SUPER_ADMIN peut modifier les permissions
			if (model.Permissions != null && !isSuperAdmin)
			{
				_logger.LogWarning($"Modification des permissions interdite : {userConnected} tente de modifier les permissions de {user.Id} depuis l'IP {ip}");
				_context.AuditLogs.Add(new AuditLog
				{
					Action = $"Échec modification permissions utilisateur {user.Id} (non super admin)",
					UserEmail = userConnected,
					Date = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris")),
					IpAddress = ip
				});
				await _context.SaveChangesAsync();
				return StatusCode(403, new { error = "Seul le SUPER_ADMIN peut modifier les permissions." });
			}

			// Mise à jour partielle des champs
			if (model.lastname != null)
				user.lastname = model.lastname;
			if (model.firstname != null)
				user.firstname = model.firstname;
			if (model.Email != null)
				user.Email = model.Email;
			if (model.Password != null)
				user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

			// Mise à jour des permissions (SUPER_ADMIN uniquement)
			if (model.Permissions != null)
			{
				var permissions = _context.Permissions.Where(p => model.Permissions.Contains(p.Name)).ToList();
				user.Permissions = permissions;
			}

			user.UpdatedAt = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris"));

			_logger.LogInformation($"Modification réussie : utilisateur {user.Id} modifié par {userConnected} depuis l'IP {ip}");
			_context.AuditLogs.Add(new AuditLog
			{
				Action = $"Modification utilisateur {user.Id} réussie",
				UserEmail = userConnected,
				Date = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris")),
				IpAddress = ip
			});

			await _context.SaveChangesAsync();

			return Ok(new { user.Id, user.Email, user.Role, Permissions = user.Permissions.Select(p => p.Name).ToList() });
		}

	}
		
}