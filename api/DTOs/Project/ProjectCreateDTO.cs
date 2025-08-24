using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using api.Enums;
using api.Binders;
using System.Text.Json.Serialization;

namespace api.DTOs
{
	public class ProjectCreateDTO
	{
		[Required]
		public string Name { get; set; }

		[Required]
		public string Overview { get; set; }

		[Required]
		public string Description { get; set; }

		public string? Slug { get; set; }

		public string? Url { get; set; }
		public string? GithubUrl { get; set; }

		[Required]
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public DeveloperRoleEnum DeveloperRole { get; set; }

		[Required]
		public StatusEnum Status { get; set; }

		public bool? IsOnline { get; set; }

		[Required]
		[ModelBinder(BinderType = typeof(JsonFormDataModelBinder))]
		public List<int> SkillIds { get; set; } = new();

		[Required]
		public List<IFormFile> Files { get; set; } = new();

		[Required]
		[ModelBinder(BinderType = typeof(JsonFormDataModelBinder))]
		public List<FileResourceDTO> FilesMeta { get; set; } = new();
	}
		
}