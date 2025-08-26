using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using api.Enums;
using api.Binders;

namespace api.DTOs
{
	public class SkillUpdateDTO
	{
		public string? Name { get; set; }
		public string? Description { get; set; }
		public string? Color { get; set; }
		public bool? IsFavorite { get; set; }
		public bool? IsHardSkill { get; set; }
		public SkillTypeEnum? Type { get; set; }
		[Range(0, 100)]
		public int? Proficiency { get; set; }
		[Url]
		public string? DocumentationUrl { get; set; }
		public List<IFormFile>? Files { get; set; }
		[ModelBinder(BinderType = typeof(JsonFormDataModelBinder))]
		public List<FileResourceDTO>? FilesMeta { get; set; }
	}
		
}