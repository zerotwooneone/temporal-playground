using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TemporalDDD.Infrastructure.ProviderCredentialing;

public class ProviderProfileConfiguration : IEntityTypeConfiguration<ProviderProfileDbo>
{
    public void Configure(EntityTypeBuilder<ProviderProfileDbo> builder)
    {
        builder.ToTable("ProviderProfiles");

        // Primary Key - auto-incrementing uint stored as INTEGER
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        // PublicId - stored as TEXT with Unique Index
        builder.Property(x => x.PublicId)
            .IsRequired(false);

        builder.HasIndex(x => x.PublicId)
            .IsUnique()
            .HasFilter("PublicId IS NOT NULL");

        // Value Objects - flattened as primitives
        builder.Property(x => x.FirstName);
        builder.Property(x => x.LastName);
        builder.Property(x => x.Email);
        builder.Property(x => x.Specialty);

        // Bool stored as INTEGER
        builder.Property(x => x.IsActive);

        // DateTimeOffset stored as Unix UTC milliseconds
        builder.Property(x => x.ActivatedAt);
        builder.Property(x => x.CreatedAt);

        // OCC Version - concurrency token
        builder.Property(x => x.Version)
            .IsConcurrencyToken();
    }
}
