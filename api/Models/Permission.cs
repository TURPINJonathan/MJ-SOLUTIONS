using System.ComponentModel.DataAnnotations;

namespace api.Models
{
	public class Permission
	{
		[Key]
		public int Id { get; set; }
		[Required]
		public required string Name { get; set; }
		public ICollection<User> Users { get; set; } = new List<User>();
	}
		
}