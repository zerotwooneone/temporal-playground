using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TemporalDDD.Infrastructure.TravelLogistics;

public class FlightBookingConfiguration : IEntityTypeConfiguration<FlightBookingDbo>
{
    public void Configure(EntityTypeBuilder<FlightBookingDbo> builder)
    {
        builder.ToTable("FlightBookings");

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

        // Value Objects - flattened as primitives
        builder.Property(x => x.FlightNumber);
        builder.Property(x => x.Origin);
        builder.Property(x => x.Destination);

        // DateTimeOffset stored as Unix UTC milliseconds
        builder.Property(x => x.DepartureTime);

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
