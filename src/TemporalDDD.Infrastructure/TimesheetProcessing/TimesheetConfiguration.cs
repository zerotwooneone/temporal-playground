using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TemporalDDD.Infrastructure.TimesheetProcessing;

public class TimesheetConfiguration : IEntityTypeConfiguration<TimesheetDbo>
{
    public void Configure(EntityTypeBuilder<TimesheetDbo> builder)
    {
        builder.ToTable("Timesheets");

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

        // DateRange Value Object - flattened as primitives
        builder.Property(x => x.PeriodStartUtc);
        builder.Property(x => x.PeriodEndUtc);

        // Hours Value Object - flattened as decimal
        builder.Property(x => x.TotalHours);

        // HourlyRate Value Object - flattened as decimal
        builder.Property(x => x.HourlyRate);

        // Money Value Objects - flattened as primitives
        builder.Property(x => x.GrossPayAmount)
            .HasColumnType("TEXT");
        builder.Property(x => x.GrossPayCurrency)
            .HasMaxLength(3);

        builder.Property(x => x.TaxAmount)
            .HasColumnType("TEXT");
        builder.Property(x => x.TaxCurrency)
            .HasMaxLength(3);

        builder.Property(x => x.NetPayAmount)
            .HasColumnType("TEXT");
        builder.Property(x => x.NetPayCurrency)
            .HasMaxLength(3);

        // Smart Enum - stored as int
        builder.Property(x => x.Status);

        // DateTimeOffset stored as Unix UTC milliseconds
        builder.Property(x => x.SubmittedAt);
        builder.Property(x => x.ProcessedAt);

        // PaymentReference Value Object - flattened as string
        builder.Property(x => x.PaymentReference);

        // RejectionReason - plain string
        builder.Property(x => x.RejectionReason)
            .IsRequired(false);
    }
}
