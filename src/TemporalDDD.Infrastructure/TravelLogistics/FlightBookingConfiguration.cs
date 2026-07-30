using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.TravelLogistics;
using TemporalDDD.Domain.TravelLogistics.ValueObjects;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.TravelLogistics;

public class FlightBookingConfiguration : IEntityTypeConfiguration<FlightBooking>
{
    public void Configure(EntityTypeBuilder<FlightBooking> builder)
    {
        builder.ToTable("FlightBookings");

        // Primary Key - auto-incrementing uint stored as INTEGER
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        // PublicId - stored as TEXT with Unique Index
        builder.Property(x => x.PublicId)
            .HasConversion(
                p => p.ToString(),
                s => FlightBookingPublicId.FromString(s))
            .IsRequired(false);

        builder.HasIndex(x => x.PublicId)
            .IsUnique()
            .HasFilter("PublicId IS NOT NULL");

        // Value Objects - flattened
        builder.Property(x => x.FlightNumber)
            .HasConversion(
                fn => fn.Value,
                s => FlightNumber.Create(s));

        builder.Property(x => x.Origin)
            .HasConversion(
                oc => oc.Value,
                s => AirportCode.Create(s));

        builder.Property(x => x.Destination)
            .HasConversion(
                dc => dc.Value,
                s => AirportCode.Create(s));

        // FlightDepartureTime Value Object - flattened
        builder.Property(x => x.DepartureTime)
            .HasConversion(
                fdt => fdt.Value,
                dt => FlightDepartureTime.Create(dt));

        // Money Value Object - flattened using ComplexProperty
        builder.ComplexProperty(x => x.Cost, c =>
        {
            c.Property(p => p.Amount)
                .HasColumnName("CostAmount")
                .HasColumnType("TEXT");

            c.Property(p => p.Currency)
                .HasColumnName("CostCurrency")
                .HasMaxLength(3);
        });

        // Smart Enum - stored as int
        builder.Property(x => x.Status)
            .HasConversion(
                bs => bs.Value,
                v => BookingStatus.FromValue(v));

        // DateTimeOffset stored as Unix UTC milliseconds
        builder.Property(x => x.BookedAt)
            .HasConversion(ValueConverters.DateTimeOffsetToUnixMillisecondsConverter);
    }
}
