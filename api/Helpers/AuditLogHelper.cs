using api.Data;
using api.Models;
using api.Helpers;

namespace api.Helpers
{
	public static class AuditLogHelper
	{
		public static void AddAudit(AppDbContext db, string action, string userEmail, string ip)
		{
			db.AuditLogs.Add(new AuditLog
			{
				Action = action,
				UserEmail = userEmail,
				Date = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris")),
				IpAddress = ip
			});
		}

	}
		
}