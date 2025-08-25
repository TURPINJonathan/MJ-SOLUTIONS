using api.Enums;

namespace api.DTOs
{
	public class CompanyResponseDTO
	{
		public int Id { get; set; }
		public required string Name { get; set; }
		public string? Address { get; set; }
		public string? City { get; set; }
		public string? Country { get; set; }
		public string? PhoneNumber { get; set; }
		public string? Email { get; set; }
		public string? Website { get; set; }
		public required string Description { get; set; }
		public string? Color { get; set; }
		public CompanyRelationTypeEnum RelationType { get; set; }
		public string RelationTypeName => Enum.GetName(typeof(CompanyRelationTypeEnum), RelationType) ?? string.Empty;
		public DateTime? ContractStartAt { get; set; }
		public DateTime? ContractEndAt { get; set; }
		public List<ProjectResponseDTO> Projects { get; set; } = new();
		public List<SkillResponseDTO> Skills { get; set; } = new();
		public List<ContactResponseDTO> Contacts { get; set; } = new();
		public List<FileResourceDTO> Files { get; set; } = new();
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public DateTime? DeletedAt { get; set; }
		public UserResponseDTO? CreatedBy { get; set; }
		public UserResponseDTO? UpdatedBy { get; set; }
		public UserResponseDTO? DeletedBy { get; set; }
	}
		
}