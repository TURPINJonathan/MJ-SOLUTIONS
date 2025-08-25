using System.ComponentModel.DataAnnotations;
using api.Enums;
using System.Text.Json.Serialization;

namespace api.DTOs
{
	public class UserResponseDTO
	{
		public int Id { get; set; }
		public required string LastName { get; set; }
		public required string FirstName { get; set; }
		public required string Email { get; set; }

		[JsonIgnore]
		public UserRoleEnum? Role { get; set; }
		public string? RoleName => Role.HasValue ? Enum.GetName(typeof(UserRoleEnum), Role.Value) : null;
		public List<string>? Permissions { get; set; }
	}
		
}