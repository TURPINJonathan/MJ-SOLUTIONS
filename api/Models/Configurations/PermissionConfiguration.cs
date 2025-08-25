using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using api.Models;

namespace api.Models.Configurations
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name).IsRequired();

            builder.HasData(
                new Permission { Id = 1, Name = "CREATE_USER" },
                new Permission { Id = 2, Name = "READ_USER" },
                new Permission { Id = 3, Name = "UPDATE_USER" },
                new Permission { Id = 4, Name = "DELETE_USER" },
                new Permission { Id = 5, Name = "CREATE_SKILL" },
                new Permission { Id = 6, Name = "READ_SKILL" },
                new Permission { Id = 7, Name = "UPDATE_SKILL" },
                new Permission { Id = 8, Name = "DELETE_SKILL" },
                new Permission { Id = 9, Name = "CREATE_PROJECT" },
                new Permission { Id = 10, Name = "READ_PROJECT" },
                new Permission { Id = 11, Name = "UPDATE_PROJECT" },
                new Permission { Id = 12, Name = "DELETE_PROJECT" },
                new Permission { Id = 13, Name = "CREATE_CONTACT" },
                new Permission { Id = 14, Name = "READ_CONTACT" },
                new Permission { Id = 15, Name = "UPDATE_CONTACT" },
                new Permission { Id = 16, Name = "DELETE_CONTACT" }
            );
        }
    }
}