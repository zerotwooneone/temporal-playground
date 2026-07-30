using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.ProviderCredentialing;

public class ProviderProfileConfiguration : IEntityTypeConfiguration<ProviderProfile>
{
    public void Configure(EntityTypeBuilder<ProviderProfile> builder)
    {
        builder.ToTable("ProviderProfiles");

        // Primary Key - auto-incrementing uint stored as INTEGER
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        // PublicId - stored as TEXT with Unique Index
        builder.Property(x => x.PublicId)
            .HasConversion(
                p => p.ToString(),
                s => ProviderPublicId.FromString(s))
            .IsRequired(false);

        builder.HasIndex(x => x.PublicId)
            .IsUnique()
            .HasFilter("PublicId IS NOT NULL");

        // Value Objects - flattened
        builder.Property(x => x.FirstName)
            .HasConversion(
                fn => fn.Value,
                s => PersonName.Create(s));

        builder.Property(x => x.LastName)
            .HasConversion(
                ln => ln.Value,
                s => PersonName.Create(s));

        builder.Property(x => x.Email)
            .HasConversion(
                e => e.Value,
                s => Email.Create(s));

        builder.Property(x => x.Specialty)
            .HasConversion(
                sp => sp.Value,
                s => Specialty.Create(s));

        // Bool stored as INTEGER
        builder.Property(x => x.IsActive)
            .HasConversion(ValueConverters.BoolToIntConverter);

        // DateTimeOffset stored as Unix UTC milliseconds
        builder.Property(x => x.ActivatedAt)
            .HasConversion(ValueConverters.DateTimeOffsetToUnixMillisecondsConverter);

        builder.Property(x => x.CreatedAt)
            .HasConversion(ValueConverters.DateTimeOffsetToUnixMillisecondsConverter);

        // OCC Version - concurrency token
        builder.Property(x => x.Version)
            .IsConcurrencyToken();
    }
}
