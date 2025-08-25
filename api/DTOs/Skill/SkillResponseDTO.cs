using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using api.Enums;
using api.Binders;
using System.Text.Json.Serialization;

namespace api.DTOs
{
	public class SkillResponseDTO
	{
		public required int Id { get; set; }
		public required string Name { get; set; }
		public required string Description { get; set; }
		public required string Color { get; set; }

		public required bool IsFavorite { get; set; }

		public required bool IsHardSkill { get; set; }

		[JsonIgnore]
		public SkillTypeEnum? Type { get; set; }
		public string? TypeName => Type.HasValue ? Enum.GetName(typeof(SkillTypeEnum), Type.Value) : string.Empty;
		public int? Proficiency { get; set; }
		public string? DocumentationUrl { get; set; }
		public List<FileResourceDTO> Files { get; set; } = new();
	}
		
}