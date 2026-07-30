using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TemporalDDD.Infrastructure.TravelLogistics;

namespace TemporalDDD.Infrastructure.TravelLogistics;

public class LodgingBookingConfiguration : IEntityTypeConfiguration<LodgingBookingDbo>
{
    public void Configure(EntityTypeBuilder<LodgingBookingDbo> builder)
    {
        builder.ToTable("LodgingBookings");

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
        builder.Property(x => x.HotelName);

        // Address Value Object - flattened as primitives
        builder.Property(x => x.AddressStreet);
        builder.Property(x => x.AddressCity);
        builder.Property(x => x.AddressState);
        builder.Property(x => x.AddressZipCode);

        // DateRange Value Object - flattened as primitives
        builder.Property(x => x.StayPeriodStartUtc);
        builder.Property(x => x.StayPeriodEndUtc);

        // Money Value Object - flattened as primitives
        builder.Property(x => x.CostAmount)
            .HasColumnType("TEXT");
        builder.Property(x => x.CostCurrency)
            .HasMaxLength(3);

        // Smart Enum - stored as int
        builder.Property(x => x.Status);

        // DateTimeOffset stored as Unix UTC milliseconds
        builder.Property(x => x.BookedAt);
    }
}
