using System.ComponentModel.DataAnnotations;

namespace api.Models
{
	public class FileResource
	{
		[Key]
		public int Id { get; set; }
		[Required]
		public string FileName { get; set; }
		public string? Description { get; set; }
		[Required]
		public string FilePath { get; set; }
		[Required]
		public string ContentType { get; set; }
		public long Size { get; set; }
		public bool? IsBanner { get; set; }
		public bool? IsLogo { get; set; }
		public bool? IsMaster { get; set; }
		public int? OwnerId { get; set; }
		public string? OwnerType { get; set; }
	}
		
}