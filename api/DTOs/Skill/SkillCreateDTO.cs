using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using api.Enums;
using api.Binders;

namespace api.DTOs
{
	public class SkillCreateDTO
	{
		[Required]
		public required string Name { get; set; }

		[Required]
		public required string Description { get; set; }

		[Required]
		public required string Color { get; set; }

		public required bool IsFavorite { get; set; }

		public required bool IsHardSkill { get; set; }

		public SkillTypeEnum? Type { get; set; }

		[Range(0, 100)]
		[Required]
		public int Proficiency { get; set; }

		[Url]
		public string? DocumentationUrl { get; set; }

		[Required]
		public required List<IFormFile> Files { get; set; }
		[Required]
		[ModelBinder(BinderType = typeof(JsonFormDataModelBinder))]
		public required List<FileResourceDTO> FilesMeta { get; set; }
	}
		
}