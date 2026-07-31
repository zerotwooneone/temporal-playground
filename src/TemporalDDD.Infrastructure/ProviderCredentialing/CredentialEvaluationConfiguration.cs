using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TemporalDDD.Infrastructure.ProviderCredentialing;

public class CredentialEvaluationConfiguration : IEntityTypeConfiguration<CredentialEvaluationDbo>
{
    public void Configure(EntityTypeBuilder<CredentialEvaluationDbo> builder)
    {
        builder.ToTable("CredentialEvaluations");

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

        // Value Objects - flattened as primitives
        builder.Property(x => x.LicenseNumber);
        builder.Property(x => x.MedicalBoard);
        builder.Property(x => x.LicenseExpiryDate);
        builder.Property(x => x.ComplianceNotes);

        // DateTimeOffset stored as Unix UTC milliseconds
        builder.Property(x => x.EvaluatedAt);

        // Smart Enum - stored as int
        builder.Property(x => x.Status);

        // WorkflowId - optional string for Temporal workflow correlation
        builder.Property(x => x.WorkflowId)
            .IsRequired(false);
    }
}
