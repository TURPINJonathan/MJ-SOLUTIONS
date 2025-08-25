using System.ComponentModel.DataAnnotations;
using api.Enums;
using System.Text.Json.Serialization;

namespace api.Models
{
	public class Skill
	{
		[Key]
		public int Id { get; set; }
		[Required]
		public required string Name { get; set; }
		[Required]
		public required string Description { get; set; }
		[Required]
		[RegularExpression(
			"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$",
			ErrorMessage = "Invalid skill type format."
			)
		]
		public required string Color { get; set; }
		public bool IsFavorite { get; set; }
		public bool IsHardSkill { get; set; }

		// Specific properties for hard skills
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public SkillTypeEnum? Type { get; set; }
		[Range(0, 100)]
		public int? Proficiency { get; set; }
		[Url]
		public string? DocumentationUrl { get; set; }
		public ICollection<FileResource>? Files { get; set; }
		public ICollection<Project>? Projects { get; set; }
		public ICollection<Company>? Companies { get; set; }

	}
	
}
