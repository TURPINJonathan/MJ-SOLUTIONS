using System.ComponentModel.DataAnnotations;
using api.Enums;
using api.Binders;
using Microsoft.AspNetCore.Mvc;

namespace api.DTOs
{
	public class CompanyCreateDTO
	{
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
		public required string Description { get; set; }
		public string? Color { get; set; }
		[Required]
		public CompanyRelationTypeEnum RelationType { get; set; }
		public DateTime? ContractStartAt { get; set; }
		public DateTime? ContractEndAt { get; set; }
		[ModelBinder(BinderType = typeof(JsonFormDataModelBinder))]
		public List<int> ProjectIds { get; set; } = new();
		[ModelBinder(BinderType = typeof(JsonFormDataModelBinder))]
		public List<int> SkillIds { get; set; } = new();
		[ModelBinder(BinderType = typeof(JsonFormDataModelBinder))]
		public List<int> ContactIds { get; set; } = new();
		public List<IFormFile>? Files { get; set; } = new List<IFormFile>();
		[ModelBinder(BinderType = typeof(JsonFormDataModelBinder))]
		public List<FileResourceDTO>? FilesMeta { get; set; } = new List<FileResourceDTO>();
	}
		
}