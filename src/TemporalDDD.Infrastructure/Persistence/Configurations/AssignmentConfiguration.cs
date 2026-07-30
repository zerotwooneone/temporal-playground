using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TemporalDDD.Domain.PlacementMatching;
using TemporalDDD.Domain.PlacementMatching.ValueObjects;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.Persistence.Configurations;

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.ToTable("Assignments");

        // Primary Key - auto-incrementing uint stored as INTEGER
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        // PublicId - stored as TEXT with Unique Index
        builder.Property(x => x.PublicId)
            .HasConversion(
                p => p.ToString(),
                s => AssignmentPublicId.FromString(s))
            .IsRequired(false);

        builder.HasIndex(x => x.PublicId)
            .IsUnique()
            .HasFilter("PublicId IS NOT NULL");

        // ProviderId - FK to ProviderProfiles
        builder.Property(x => x.ProviderId)
            .HasConversion(
                pid => pid.Value,
                v => ProviderId.FromDatabase(v));

        // FacilityId - stored as uint
        builder.Property(x => x.FacilityId)
            .HasConversion(
                fid => fid.Value,
                v => FacilityId.FromDatabase(v));

        // PositionId - stored as uint
        builder.Property(x => x.PositionId)
            .HasConversion(
                pid => pid.Value,
                v => PositionId.FromDatabase(v));

        // Value Object - MatchScore flattened
        builder.Property(x => x.MatchScore)
            .HasConversion(
                ms => ms.Value,
                v => MatchScore.Create(v));

        // Smart Enum - stored as int
        builder.Property(x => x.Status)
            .HasConversion(
                status => status.Value,
                v => AssignmentStatus.FromValue(v));

        // DateTimeOffset stored as Unix UTC milliseconds
        builder.Property(x => x.ProposedAt)
            .HasConversion(ValueConverters.DateTimeOffsetToUnixMillisecondsConverter);

        builder.Property(x => x.AcceptedAt)
            .HasConversion(ValueConverters.DateTimeOffsetToUnixMillisecondsConverter);

        // OCC Version - concurrency token
        builder.Property(x => x.Version)
            .IsConcurrencyToken();
    }
}
