using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using api.Enums;

namespace api.Models
{
	public class User
	{
		[Key]
		public int Id { get; set; }

		[Required]
		[MaxLength(250)]
		public required string lastname { get; set; }

		[Required]
		[MaxLength(250)]
		public required string firstname { get; set; }

		[Required]
		[EmailAddress]
		public required string Email { get; set; }

		[Required]
		public required string PasswordHash { get; set; }

		public UserRoleEnum? Role { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public int JwtVersion { get; set; } = 1;
		public ICollection<Permission> Permissions { get; set; } = new List<Permission>();

	}
}