using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using api.Enums;
using api.Binders;
using System.Text.Json.Serialization;

namespace api.DTOs
{
	public class ProjectResponseDTO
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string Overview { get; set; }

		public string Description { get; set; }

		public string? Slug { get; set; }

		public string? Url { get; set; }
		public string? GithubUrl { get; set; }
		[JsonIgnore]
		public DeveloperRoleEnum DeveloperRole { get; set; }
		public string DeveloperRoleName => Enum.GetName(typeof(DeveloperRoleEnum), DeveloperRole);
		[JsonIgnore]
		public StatusEnum Status { get; set; }
		public string StatusName => Enum.GetName(typeof(StatusEnum), Status);

		public bool? IsOnline { get; set; }
		public List<SkillResponseDTO> Skills { get; set; } = new();
		public List<FileResourceDTO> Files { get; set; } = new();

		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public DateTime? DeletedAt { get; set; }
		public DateTime? PublishedAt { get; set; }
		public UserResponseDTO? CreatedBy { get; set; }
		public UserResponseDTO? UpdatedBy { get; set; }
		public UserResponseDTO? DeletedBy { get; set; }
		public UserResponseDTO? PublishedBy { get; set; }
	}
		
}