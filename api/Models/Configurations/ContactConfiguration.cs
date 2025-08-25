using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using api.Models;
using System;

namespace api.Models.Configurations
{
	public class ContactConfiguration : IEntityTypeConfiguration<Contact>
	{
		public void Configure(EntityTypeBuilder<Contact> builder)
		{
			builder.Property(c => c.FirstName)
				.IsRequired();

			builder.Property(c => c.LastName)
				.IsRequired();

			builder.Property(c => c.Email)
				.IsRequired()
				.HasDefaultValue(string.Empty);

			builder.Property(c => c.PhoneNumber)
				.HasMaxLength(50);

			builder.Property(c => c.Position)
				.HasMaxLength(200);

			builder.Property(c => c.Note)
				.HasColumnType("TEXT");

			builder.Property(c => c.CreatedAt)
				.HasColumnType("timestamp")
				.HasDefaultValueSql("CURRENT_TIMESTAMP");
		}

	}
	
}