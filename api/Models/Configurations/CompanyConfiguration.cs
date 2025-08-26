using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using api.Models;

namespace api.Models.Configurations
{
	public class CompanyConfiguration : IEntityTypeConfiguration<Company>
	{
		public void Configure(EntityTypeBuilder<Company> builder)
		{
			builder.Property(c => c.Name)
					.IsRequired()
					.HasMaxLength(250);

			builder.Property(c => c.Address)
					.HasMaxLength(250);

			builder.Property(c => c.City)
					.HasMaxLength(100);

			builder.Property(c => c.Country)
					.HasMaxLength(100);

			builder.Property(c => c.PhoneNumber)
					.HasMaxLength(50);

			builder.Property(c => c.Email)
					.HasMaxLength(250);

			builder.Property(c => c.Website)
					.HasMaxLength(250);

			builder.Property(c => c.Description)
					.IsRequired()
					.HasColumnType("TEXT");

			builder.Property(c => c.Color)
					.HasMaxLength(7);

			builder.Property(c => c.RelationType)
					.HasConversion<string>()
					.IsRequired();

			builder.Property(c => c.ContractStartAt)
					.HasColumnType("timestamp");

			builder.Property(c => c.ContractEndAt)
					.HasColumnType("timestamp");

			builder.Property(c => c.CreatedAt)
					.HasColumnType("timestamp")
					.HasDefaultValueSql("CURRENT_TIMESTAMP");

			builder.HasMany(c => c.Files)
					.WithOne()
					.HasForeignKey("CompanyId")
					.OnDelete(DeleteBehavior.Cascade);

			builder
					.HasMany(c => c.Projects)
					.WithMany(p => p.Companies)
					.UsingEntity(j => j.ToTable("CompanyProjects"));

			builder
					.HasMany(c => c.Skills)
					.WithMany(s => s.Companies)
					.UsingEntity(j => j.ToTable("CompanySkills"));

			builder
					.HasMany(c => c.Contacts)
					.WithMany(ct => ct.Companies)
					.UsingEntity(j => j.ToTable("CompanyContacts"));

			builder.HasOne<User>()
					.WithMany()
					.HasForeignKey(c => c.CreatedById)
					.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne<User>()
					.WithMany()
					.HasForeignKey(c => c.UpdatedById)
					.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne<User>()
					.WithMany()
					.HasForeignKey(c => c.DeletedById)
					.OnDelete(DeleteBehavior.Restrict);
		}

	}
		
}