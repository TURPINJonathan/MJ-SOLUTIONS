using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
	public class Contact
	{
		[Key]
		public int Id { get; set; }
		[Required]
		public required string FirstName { get; set; }
		[Required]
		public required string LastName { get; set; }
		[EmailAddress]
		public string? Email { get; set; }
		public string? PhoneNumber { get; set; }
		public string? Position { get; set; }
		[Column(TypeName = "TEXT")]
		public string? Note { get; set; }
		public ICollection<FileResource>? Files { get; set; } = new List<FileResource>();
		public ICollection<Company>? Companies { get; set; } = new List<Company>();
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public DateTime? DeletedAt { get; set; }

	}
		
}