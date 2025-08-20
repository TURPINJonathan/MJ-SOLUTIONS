using System.ComponentModel.DataAnnotations;

namespace api.Models
{
	public class Permission
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public ICollection<User> Users { get; set; }
	}
		
}