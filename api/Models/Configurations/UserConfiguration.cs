using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using api.Models;
using System;

namespace api.Models.Configurations
{
	public class UserConfiguration : IEntityTypeConfiguration<User>
	{
		public void Configure(EntityTypeBuilder<User> builder)
		{
			builder.HasIndex(s => s.Email).IsUnique();

			builder.Property(s => s.lastname)
					.IsRequired()
					.HasDefaultValue(string.Empty)
					.HasMaxLength(250);

			builder.Property(s => s.firstname)
					.IsRequired()
					.HasDefaultValue(string.Empty)
					.HasMaxLength(250);

			builder.Property(s => s.Email)
					.IsRequired()
					.HasDefaultValue(string.Empty)
					.HasMaxLength(250);

			builder.Property(s => s.PasswordHash)
					.IsRequired()
					.HasDefaultValue(string.Empty)
					.HasMaxLength(250);

			builder.Property(s => s.Role)
					.HasConversion<string>();

			builder.Property(s => s.CreatedAt)
					.HasColumnType("timestamp")
					.HasDefaultValueSql("CURRENT_TIMESTAMP");

			builder.Property(s => s.UpdatedAt)
					.HasColumnType("timestamp")
					.HasDefaultValueSql("CURRENT_TIMESTAMP");
			}
	}
}