using System.ComponentModel.DataAnnotations;
using api.Binders;
using Microsoft.AspNetCore.Mvc;

namespace api.DTOs
{
	public class ContactCreateDTO
	{
		[Required]
		public required string FirstName { get; set; }
		[Required]
		public required string LastName { get; set; }
		[EmailAddress]
		public string? Email { get; set; }
		public string? PhoneNumber { get; set; }
		public string? Position { get; set; }
		public string? Note { get; set; }
		public List<IFormFile>? Files { get; set; } = new List<IFormFile>();
		[ModelBinder(BinderType = typeof(JsonFormDataModelBinder))]
		public List<FileResourceDTO>? FilesMeta { get; set; } = new List<FileResourceDTO>();
		public DateTime CreatedAt { get; set; }
	}

}