using System.ComponentModel.DataAnnotations;
using api.Enums;
using System.Text.Json.Serialization;

namespace api.Models
{
	public class AuditLog
	{
		public int Id { get; set; }
		[Required]
		public required string Action { get; set; }
		[Required]
		public required string UserEmail { get; set; }
		[Required]
		public required DateTime Date { get; set; }
		[Required]
		public required string IpAddress { get; set; }
		[Required]
		public required string OwnerType { get; set; }
		public int? OwnerId { get; set; }
	}

}
