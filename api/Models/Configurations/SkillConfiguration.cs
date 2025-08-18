using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using api.Models;

namespace api.Models.Configurations
{
	public class SkillConfiguration : IEntityTypeConfiguration<Skill>
	{
			public void Configure(EntityTypeBuilder<Skill> builder)
			{
				builder.HasIndex(s => s.Name).IsUnique();
				builder.HasIndex(s => s.Color).IsUnique();

				builder.Property(s => s.Name)
						.IsRequired()
						.HasDefaultValue(string.Empty)
						.HasMaxLength(250);

				builder.Property(s => s.Description)
						.IsRequired()
						.HasDefaultValue(string.Empty)
						.HasColumnType("TEXT");

				builder.Property(s => s.Color)
						.IsRequired()
						.HasDefaultValue("#FFFFFF")
						.HasMaxLength(7);

				builder.Property(s => s.IsFavorite)
						.HasDefaultValue(false);

				builder.Property(s => s.IsHardSkill)
						.HasDefaultValue(false);

				builder.Property(s => s.Type)
						.HasConversion<string>();

				builder.Property(s => s.Proficiency)
						.HasDefaultValue(0);

			}	
	}
}