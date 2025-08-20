using api.Data;
using api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace api.Helpers
{
	public static class UserHelper
	{
		public static User GetConnectedUserWithPermissions(HttpContext context, AppDbContext db)
		{
			var email = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
			return db.Users
					.Include(u => u.Permissions)
					.FirstOrDefault(u => u.Email == email);
		}

		public static bool HasPermission(HttpContext context, AppDbContext db, string permissionName)
		{
			var user = GetConnectedUserWithPermissions(context, db);
			return user != null && user.Permissions.Any(p => p.Name == permissionName);
		}
		
	}

}