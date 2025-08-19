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
		public string lastname { get; set; }

		[Required]
		[MaxLength(250)]
		public string firstname { get; set; }

		[Required]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		public string PasswordHash { get; set; }

		public UserRole? Role { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }

	}
}