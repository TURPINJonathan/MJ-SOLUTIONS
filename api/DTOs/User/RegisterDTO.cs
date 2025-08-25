using System.ComponentModel.DataAnnotations;
using api.Enums;
using System.Text.Json.Serialization;

namespace api.DTOs
{
	public class RegisterDTO
	{
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
		[MinLength(8)]
		[RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[\W_]).+$", ErrorMessage = "Password must be at least 8 characters long and contain a mix of uppercase, lowercase, numeric, and special characters.")]
		public required string Password { get; set; }

		[JsonConverter(typeof(JsonStringEnumConverter))]
		public UserRoleEnum? Role { get; set; }
		public List<string>? Permissions { get; set; }
	}
		
}