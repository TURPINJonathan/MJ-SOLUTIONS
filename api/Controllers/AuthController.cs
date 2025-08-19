using Microsoft.AspNetCore.Mvc;
using api.Models;
using api.Data;
using api.Enums;
using BCrypt.Net;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;

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
        // [Authorize(Roles = "SUPER_ADMIN")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (_context.Users.Any(u => u.Email == model.Email))
                return BadRequest(new { error = "Cet email est déjà utilisé." });

            if (string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { error = "Le mot de passe est obligatoire." });

            if (model.Password.Length < 8)
                return BadRequest(new { error = "Le mot de passe doit contenir au moins 8 caractères." });

            if (!model.Password.Any(char.IsUpper))
                return BadRequest(new { error = "Le mot de passe doit contenir au moins une majuscule." });

            if (!model.Password.Any(char.IsLower))
                return BadRequest(new { error = "Le mot de passe doit contenir au moins une minuscule." });

            if (!model.Password.Any(char.IsDigit))
                return BadRequest(new { error = "Le mot de passe doit contenir au moins un chiffre." });

            if (!model.Password.Any(ch => !char.IsLetterOrDigit(ch)))
                return BadRequest(new { error = "Le mot de passe doit contenir au moins un caractère spécial." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new User
            {
                lastname = model.lastname,
                firstname = model.firstname,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = model.Role ?? UserRole.USER,
                CreatedAt = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris")),
                UpdatedAt = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris"))
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Nouvel utilisateur créé : {user.Email}");

            return Ok(new { user.Id, user.Email, user.Role });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel login)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            _logger.LogInformation($"Tentative de connexion pour {login.Email} depuis l'ip {ip}");

            if (!loginAttempts.ContainsKey(ip))
                loginAttempts[ip] = (0, null);

            // Blocage temporaire après 5 tentatives
            if (loginAttempts[ip].blockedUntil.HasValue && loginAttempts[ip].blockedUntil > DateTime.UtcNow)
            {
                _logger.LogWarning($"Connexion bloquée pour {login.Email} depuis {ip} jusqu'à {loginAttempts[ip].blockedUntil.Value.ToLocalTime()}");
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
                    _logger.LogWarning($"Échec de connexion pour {login.Email} depuis l'ip {ip}.");
                    _logger.LogWarning($"Blocage de l'ip {ip} après 5 échecs de connexion.");
                    blockedUntil = DateTime.UtcNow.AddMinutes(10); // Bloque 10 minutes
                }

                loginAttempts[ip] = (count, blockedUntil);

                return Unauthorized(new { error = "Email ou mot de passe incorrect." });
            }

            // Si login réussi, reset le compteur
            loginAttempts[ip] = (0, null);

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
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds);

            // Génération du refresh token
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var expiryDate = DateTime.UtcNow.AddDays(7);

            _context.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiryDate = expiryDate
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
            var tokenEntry = _context.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken && rt.ExpiryDate > DateTime.UtcNow);
            if (tokenEntry == null)
                return Unauthorized(new { error = "Refresh token invalide ou expiré." });

            var user = _context.Users.Find(tokenEntry.UserId);
            if (user == null)
            {
                _logger.LogWarning($"Refresh token utilisé pour un utilisateur non trouvé. Token: {refreshToken}");
                return Unauthorized(new { error = "Utilisateur non trouvé." });
            }

            // Supprime le refresh token utilisé
            _context.RefreshTokens.Remove(tokenEntry);
            _context.SaveChanges();

            // Génère un nouveau refresh token
            var newRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var expiryDate = DateTime.UtcNow.AddDays(7);

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
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds);

            _logger.LogInformation($"Refresh token utilisé pour {user.Email}");

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken = newRefreshToken
            });
        }
    }
}