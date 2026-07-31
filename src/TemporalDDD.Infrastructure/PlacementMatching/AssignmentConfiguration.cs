using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TemporalDDD.Infrastructure.PlacementMatching;

public class AssignmentConfiguration : IEntityTypeConfiguration<AssignmentDbo>
{
    public void Configure(EntityTypeBuilder<AssignmentDbo> builder)
    {
        builder.ToTable("Assignments");

        // Primary Key - client-generated string ID
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .IsRequired();

        // PublicId - stored as TEXT with Unique Index
        builder.Property(x => x.PublicId)
            .IsRequired(false);

        builder.HasIndex(x => x.PublicId)
            .IsUnique()
            .HasFilter("PublicId IS NOT NULL");

        // ProviderId - FK to ProviderProfiles (uint)
        builder.Property(x => x.ProviderId);

        // FacilityId - stored as uint
        builder.Property(x => x.FacilityId);

        // PositionId - stored as uint
        builder.Property(x => x.PositionId);

        // MatchScore - stored as decimal
        builder.Property(x => x.MatchScore);

        // Status - stored as int
        builder.Property(x => x.Status);

        // DateTimeOffset stored as Unix UTC milliseconds
        builder.Property(x => x.ProposedAt);

        builder.Property(x => x.AcceptedAt);

        // OCC Version - concurrency token
        builder.Property(x => x.Version)
            .IsConcurrencyToken();
    }
}
