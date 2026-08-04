using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TemporalDDD.Infrastructure.IdentityAndAccess;

public class UserConfiguration : IEntityTypeConfiguration<UserDbo>
{
    public void Configure(EntityTypeBuilder<UserDbo> builder)
    {
        builder.ToTable("Identity_Users");

        // Primary Key - client-generated string ID
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .IsRequired();

        // PublicId - stored as TEXT with Unique Index
        builder.Property(x => x.PublicId)
            .IsRequired();

        builder.HasIndex(x => x.PublicId)
            .IsUnique();

        // Other properties
        builder.Property(x => x.Username)
            .IsRequired();

        builder.Property(x => x.Email)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        // AssignedRoles stored as JSON string
        builder.Property(x => x.AssignedRolesJson)
            .IsRequired();
    }
}
