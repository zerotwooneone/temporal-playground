using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TemporalDDD.Infrastructure.IdentityAndAccess;

public class RoleConfiguration : IEntityTypeConfiguration<RoleDbo>
{
    public void Configure(EntityTypeBuilder<RoleDbo> builder)
    {
        builder.ToTable("Identity_Roles");

        // Primary Key - client-generated string ID
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .IsRequired();

        // Other properties
        builder.Property(x => x.Name)
            .IsRequired();

        builder.Property(x => x.Description)
            .IsRequired();

        // Permissions stored as JSON string
        builder.Property(x => x.PermissionsJson)
            .IsRequired();
    }
}
