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
		public DbSet<FileResource> FileResources { get; set; }
		public DbSet<Project> Projects { get; set; }
		public DbSet<Contact> Contacts { get; set; }
		public DbSet<Company> Companies { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfiguration(new UserConfiguration());
			modelBuilder.ApplyConfiguration(new SkillConfiguration());
			modelBuilder.ApplyConfiguration(new PermissionConfiguration());
			modelBuilder.ApplyConfiguration(new ProjectConfiguration());
			modelBuilder.ApplyConfiguration(new ContactConfiguration());
			modelBuilder.ApplyConfiguration(new CompanyConfiguration());
		}
		
	}
}
