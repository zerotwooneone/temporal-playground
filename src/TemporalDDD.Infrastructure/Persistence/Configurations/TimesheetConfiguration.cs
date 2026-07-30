using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.TimesheetProcessing;
using TemporalDDD.Domain.TimesheetProcessing.ValueObjects;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.Persistence.Configurations;

public class TimesheetConfiguration : IEntityTypeConfiguration<Timesheet>
{
    public void Configure(EntityTypeBuilder<Timesheet> builder)
    {
        builder.ToTable("Timesheets");

        // Primary Key - auto-incrementing uint stored as INTEGER
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        // PublicId - stored as TEXT with Unique Index
        builder.Property(x => x.PublicId)
            .HasConversion(
                p => p.ToString(),
                s => TimesheetPublicId.FromString(s))
            .IsRequired(false);

        builder.HasIndex(x => x.PublicId)
            .IsUnique()
            .HasFilter("PublicId IS NOT NULL");

        // ProviderId - FK to ProviderProfiles
        builder.Property(x => x.ProviderId)
            .HasConversion(
                pid => pid.Value,
                v => ProviderId.Create(v).Value!);

        // DateRange Value Object - flattened using ComplexProperty
        builder.ComplexProperty(x => x.Period, pb =>
        {
            pb.Property(p => p.Start)
                .HasColumnName("PeriodStartUtc")
                .HasConversion(ValueConverters.DateTimeToUnixMillisecondsConverter);

            pb.Property(p => p.End)
                .HasColumnName("PeriodEndUtc")
                .HasConversion(ValueConverters.DateTimeToUnixMillisecondsConverter);
        });

        // Hours Value Object - flattened
        builder.Property(x => x.TotalHours)
            .HasConversion(
                h => h.Value,
                v => Hours.Create(v));

        // HourlyRate Value Object - flattened
        builder.Property(x => x.HourlyRate)
            .HasConversion(
                hr => hr.Value,
                v => HourlyRate.Create(v));

        // Money Value Objects - flattened using ComplexProperty
        builder.ComplexProperty(x => x.GrossPay, gp =>
        {
            gp.Property(p => p.Amount)
                .HasColumnName("GrossPayAmount")
                .HasColumnType("TEXT");

            gp.Property(p => p.Currency)
                .HasColumnName("GrossPayCurrency")
                .HasMaxLength(3);
        });

        builder.ComplexProperty(x => x.TaxAmount, ta =>
        {
            ta.Property(p => p.Amount)
                .HasColumnName("TaxAmount")
                .HasColumnType("TEXT");

            ta.Property(p => p.Currency)
                .HasColumnName("TaxCurrency")
                .HasMaxLength(3);
        });

        builder.ComplexProperty(x => x.NetPay, np =>
        {
            np.Property(p => p.Amount)
                .HasColumnName("NetPayAmount")
                .HasColumnType("TEXT");

            np.Property(p => p.Currency)
                .HasColumnName("NetPayCurrency")
                .HasMaxLength(3);
        });

        // Smart Enum - stored as int
        builder.Property(x => x.Status)
            .HasConversion(
                ts => ts.Value,
                v => TimesheetStatus.FromValue(v));

        // DateTimeOffset stored as Unix UTC milliseconds
        builder.Property(x => x.SubmittedAt)
            .HasConversion(ValueConverters.DateTimeOffsetToUnixMillisecondsConverter);

        builder.Property(x => x.ProcessedAt)
            .HasConversion(ValueConverters.DateTimeOffsetToUnixMillisecondsConverter);

        // PaymentReference Value Object - flattened
        builder.Property(x => x.PaymentReference)
            .HasConversion(
                pr => pr.Value,
                s => PaymentReference.Create(s));

        // RejectionReason - plain string
        builder.Property(x => x.RejectionReason)
            .IsRequired(false);
    }
}
