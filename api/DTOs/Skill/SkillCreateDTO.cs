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
		public string Name { get; set; }

		[Required]
		public string Description { get; set; }

		[Required]
		public string Color { get; set; }

		public bool IsFavorite { get; set; }

		public bool IsHardSkill { get; set; }

		public SkillType? Type { get; set; }

		[Range(0, 100)]
		[Required]
		public int? Proficiency { get; set; }

		[Url]
		public string? DocumentationUrl { get; set; }

		[Required]
		public List<IFormFile> Files { get; set; }

		[ModelBinder(BinderType = typeof(JsonFormDataModelBinder))]
		public List<FileResourceMeta> FilesMeta { get; set; }
	}
		
}