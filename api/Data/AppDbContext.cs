using Microsoft.EntityFrameworkCore;
using api.Models;
using api.Models.Configurations;

namespace api.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<User> Users { get; set; }
		public DbSet<Permission> Permissions { get; set; }
		public DbSet<RefreshToken> RefreshTokens { get; set; }
		public DbSet<BlacklistedToken> BlacklistedTokens { get; set; }
		public DbSet<AuditLog> AuditLogs { get; set; }
		public DbSet<Skill> Skills { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfiguration(new UserConfiguration());
			modelBuilder.ApplyConfiguration(new SkillConfiguration());

			modelBuilder.Entity<Permission>().HasData(
				new Permission { Id = 1, Name = "CREATE_USER" },
				new Permission { Id = 2, Name = "READ_USER" },
				new Permission { Id = 3, Name = "UPDATE_USER" },
				new Permission { Id = 4, Name = "DELETE_USER" },
				new Permission { Id = 5, Name = "CREATE_SKILL" },
				new Permission { Id = 6, Name = "READ_SKILL" },
				new Permission { Id = 7, Name = "UPDATE_SKILL" },
				new Permission { Id = 8, Name = "DELETE_SKILL" }
			);
		}
		
	}
}
