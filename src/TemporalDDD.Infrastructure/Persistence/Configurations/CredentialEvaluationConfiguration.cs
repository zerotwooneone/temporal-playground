using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.Persistence.Configurations;

public class CredentialEvaluationConfiguration : IEntityTypeConfiguration<CredentialEvaluation>
{
    public void Configure(EntityTypeBuilder<CredentialEvaluation> builder)
    {
        builder.ToTable("CredentialEvaluations");

        // Primary Key - auto-incrementing uint stored as INTEGER
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        // PublicId - stored as TEXT with Unique Index
        builder.Property(x => x.PublicId)
            .HasConversion(
                p => p.ToString(),
                s => CredentialEvaluationPublicId.FromString(s))
            .IsRequired(false);

        builder.HasIndex(x => x.PublicId)
            .IsUnique()
            .HasFilter("PublicId IS NOT NULL");

        // ProviderId - FK to ProviderProfiles
        builder.Property(x => x.ProviderId)
            .HasConversion(
                pid => pid.Value,
                v => ProviderId.FromDatabase(v));

        // Value Objects - flattened
        builder.Property(x => x.LicenseNumber)
            .HasConversion(
                ln => ln.Value,
                s => LicenseNumber.Create(s));

        builder.Property(x => x.MedicalBoard)
            .HasConversion(
                mb => mb.Value,
                s => MedicalBoard.Create(s));

        builder.Property(x => x.LicenseExpiryDate)
            .HasConversion(
                led => led.Value,
                dt => LicenseExpiryDate.Create(dt));

        builder.Property(x => x.ComplianceNotes)
            .HasConversion(
                cn => cn.Value,
                s => ComplianceNotes.Create(s));

        // DateTimeOffset stored as Unix UTC milliseconds
        builder.Property(x => x.EvaluatedAt)
            .HasConversion(ValueConverters.DateTimeOffsetToUnixMillisecondsConverter);

        // Smart Enum - stored as int
        builder.Property(x => x.Status)
            .HasConversion(
                es => es.Value,
                v => EvaluationStatus.FromValue(v));
    }
}
