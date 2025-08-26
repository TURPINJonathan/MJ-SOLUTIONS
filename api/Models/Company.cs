using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using api.Enums;
using api.Interface;

namespace api.Models
{
	public class Company : IUserTrackable
	{
		[Key]
		public int Id { get; set; }
		[Required]
		public required string Name { get; set; }
		public string? Address { get; set; }
		public string? City { get; set; }
		public string? Country { get; set; }
		public string? PhoneNumber { get; set; }
		[EmailAddress]
		public string? Email { get; set; }
		[Url]
		public string? Website { get; set; }
		[Required]
		[Column(TypeName = "TEXT")]
		public required string Description { get; set; }
		[RegularExpression(
			"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$",
			ErrorMessage = "Invalid company color format."
			)
		]
		public string? Color { get; set; }
		[Required]
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public CompanyRelationTypeEnum RelationType { get; set; }
		public ICollection<FileResource>? Files { get; set; } = new List<FileResource>();
		public ICollection<Project>? Projects { get; set; } = new List<Project>();
		public ICollection<Skill>? Skills { get; set; } = new List<Skill>();
		public ICollection<Contact>? Contacts { get; set; } = new List<Contact>();
		public DateTime? ContractStartAt { get; set; }
		public DateTime? ContractEndAt { get; set; }
		[Required]
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public DateTime? DeletedAt { get; set; }
		[Required]
		public int CreatedById { get; set; }
		public int? UpdatedById { get; set; }
		public int? DeletedById { get; set; }

	}

}