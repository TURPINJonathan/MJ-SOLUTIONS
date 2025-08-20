using System.ComponentModel.DataAnnotations;
using api.Enums;
using System.Text.Json.Serialization;

namespace api.Models
{
	public class AuditLog
	{
		public int Id { get; set; }
		public string Action { get; set; }
		public string UserEmail { get; set; }
		public DateTime Date { get; set; }
		public string IpAddress { get; set; }
	}

}
