using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using api.Models;

namespace api.Models.Configurations
{
	public class ProjectConfiguration : IEntityTypeConfiguration<Project>
	{
		public void Configure(EntityTypeBuilder<Project> builder)
		{
			builder.Property(p => p.Name)
					.IsRequired()
					.HasMaxLength(250);

			builder.Property(p => p.Overview)
					.IsRequired();

			builder.Property(p => p.Description)
					.IsRequired();

			builder.Property(p => p.Slug)
					.IsRequired();

			builder.Property(p => p.Url)
					.IsRequired(false);

			builder.Property(p => p.GithubUrl)
					.IsRequired(false);

			builder.Property(p => p.DeveloperRole)
					.HasConversion<string>()
					.IsRequired();

			builder.Property(p => p.IsOnline)
					.IsRequired(false);

			builder.Property(p => p.Status)
					.HasConversion<string>();

			builder
					.HasMany(p => p.Skills)
					.WithMany(s => s.Projects)
					.UsingEntity(j => j.ToTable("ProjectSkills"));

			builder.HasMany(p => p.Files)
					.WithOne()
					.HasForeignKey("ProjectId")
					.OnDelete(DeleteBehavior.Cascade);

			builder.Property(p => p.CreatedAt)
					.HasColumnType("timestamp")
					.HasDefaultValueSql("CURRENT_TIMESTAMP");

			builder.Property(p => p.UpdatedAt)
					.HasColumnType("timestamp");

			builder.Property(p => p.DeletedAt)
					.HasColumnType("timestamp");

			builder.Property(p => p.PublishedAt)
					.HasColumnType("timestamp");

			builder.HasOne<User>()
					.WithMany()
					.HasForeignKey(p => p.CreatedById)
					.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne<User>()
					.WithMany()
					.HasForeignKey(p => p.UpdatedById)
					.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne<User>()
					.WithMany()
					.HasForeignKey(p => p.DeletedById)
					.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne<User>()
					.WithMany()
					.HasForeignKey(p => p.PublishedById)
					.OnDelete(DeleteBehavior.Restrict);

		}

	}
	
}