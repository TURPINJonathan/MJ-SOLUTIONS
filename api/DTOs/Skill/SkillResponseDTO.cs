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
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string Color { get; set; }

		public bool IsFavorite { get; set; }

		public bool IsHardSkill { get; set; }

		[JsonIgnore]
		public SkillTypeEnum? Type { get; set; }
		public string? TypeName => Enum.GetName(typeof(SkillTypeEnum), Type);
		public int? Proficiency { get; set; }
		public string? DocumentationUrl { get; set; }
		public List<FileResourceDTO> Files { get; set; } = new();
	}
		
}