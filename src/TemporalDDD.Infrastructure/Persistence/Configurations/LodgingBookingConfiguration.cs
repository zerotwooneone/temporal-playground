using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.TravelLogistics;
using TemporalDDD.Domain.TravelLogistics.ValueObjects;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.Persistence.Configurations;

public class LodgingBookingConfiguration : IEntityTypeConfiguration<LodgingBooking>
{
    public void Configure(EntityTypeBuilder<LodgingBooking> builder)
    {
        builder.ToTable("LodgingBookings");

        // Primary Key - auto-incrementing uint stored as INTEGER
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        // PublicId - stored as TEXT with Unique Index
        builder.Property(x => x.PublicId)
            .HasConversion(
                p => p.ToString(),
                s => LodgingBookingPublicId.FromString(s))
            .IsRequired(false);

        builder.HasIndex(x => x.PublicId)
            .IsUnique()
            .HasFilter("PublicId IS NOT NULL");

        // Value Objects - flattened
        builder.Property(x => x.HotelName)
            .HasConversion(
                hn => hn.Value,
                s => HotelName.Create(s));

        // Address Value Object - flattened using ComplexProperty
        builder.ComplexProperty(x => x.Address, a =>
        {
            a.Property(p => p.Street)
                .HasColumnName("AddressStreet");

            a.Property(p => p.City)
                .HasColumnName("AddressCity");

            a.Property(p => p.State)
                .HasColumnName("AddressState");

            a.Property(p => p.ZipCode)
                .HasColumnName("AddressZipCode");
        });

        // DateRange Value Object - flattened using ComplexProperty
        builder.ComplexProperty(x => x.StayPeriod, sp =>
        {
            sp.Property(p => p.Start)
                .HasColumnName("StayPeriodStartUtc")
                .HasConversion(ValueConverters.DateTimeToUnixMillisecondsConverter);

            sp.Property(p => p.End)
                .HasColumnName("StayPeriodEndUtc")
                .HasConversion(ValueConverters.DateTimeToUnixMillisecondsConverter);
        });

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
