using System.ComponentModel.DataAnnotations;
using api.Enums;
using System.Text.Json.Serialization;

namespace api.DTOs
{
	public class UpdateUserDTO
	{
		public int UserId { get; set; }
		
		[MaxLength(250)]
		public string? lastname { get; set; }

		[MaxLength(250)]
		public string? firstname { get; set; }

		[EmailAddress]
		public string? Email { get; set; }

		[MinLength(8)]
		[RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[\W_]).+$", ErrorMessage = "Password must be at least 8 characters long and contain a mix of uppercase, lowercase, numeric, and special characters.")]
		public string? Password { get; set; }

		[JsonConverter(typeof(JsonStringEnumConverter))]
		public UserRoleEnum? Role { get; set; }
		public List<string>? Permissions { get; set; }
	}
		
}