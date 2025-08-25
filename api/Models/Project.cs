using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using api.Enums;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace api.Models
{
	public class Project
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public required string Name { get; set; }

		[Required]
		public required string Overview { get; set; }

		[Required]
		[Column(TypeName = "TEXT")]
		public required string Description { get; set; }

		[Required]
		public required string Slug { get; set; }

		public string? Url { get; set; }

		public string? GithubUrl { get; set; }

		[Required]
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public DeveloperRoleEnum DeveloperRole { get; set; }

		[Required]
		public StatusEnum Status { get; set; }

		public bool? IsOnline { get; set; }

		public ICollection<Skill> Skills { get; set; } = new List<Skill>();
		public ICollection<Company> Companies { get; set; } = new List<Company>();

		public ICollection<FileResource> Files { get; set; } = new List<FileResource>();

		[Required]
		public DateTime CreatedAt { get; set; }

		public DateTime? UpdatedAt { get; set; }

		public DateTime? DeletedAt { get; set; }

		public DateTime? PublishedAt { get; set; }

		[Required]
		public int CreatedById { get; set; }
		
		public int? UpdatedById { get; set; }
		
		public int? DeletedById { get; set; }
		
		public int? PublishedById { get; set; }

	}
}