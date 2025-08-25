using api.Enums;
using api.Binders;
using Microsoft.AspNetCore.Mvc;

namespace api.DTOs
{
	public class CompanyUpdateDTO
	{
		public string? Name { get; set; }
		public string? Address { get; set; }
		public string? City { get; set; }
		public string? Country { get; set; }
		public string? PhoneNumber { get; set; }
		public string? Email { get; set; }
		public string? Website { get; set; }
		public string? Description { get; set; }
		public string? Color { get; set; }
		public CompanyRelationTypeEnum? RelationType { get; set; }
		public DateTime? ContractStartAt { get; set; }
		public DateTime? ContractEndAt { get; set; }
		[ModelBinder(BinderType = typeof(JsonFormDataModelBinder))]
		public List<int>? ProjectIds { get; set; }
		[ModelBinder(BinderType = typeof(JsonFormDataModelBinder))]
		public List<int>? SkillIds { get; set; }
		[ModelBinder(BinderType = typeof(JsonFormDataModelBinder))]
		public List<int>? ContactIds { get; set; }
		public List<IFormFile>? Files { get; set; } = new List<IFormFile>();
		[ModelBinder(BinderType = typeof(JsonFormDataModelBinder))]
		public List<FileResourceDTO>? FilesMeta { get; set; } = new List<FileResourceDTO>();
	}
		
}