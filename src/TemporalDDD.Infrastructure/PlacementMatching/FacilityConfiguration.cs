using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TemporalDDD.Infrastructure.PlacementMatching;

public class FacilityConfiguration : IEntityTypeConfiguration<FacilityDbo>
{
    public void Configure(EntityTypeBuilder<FacilityDbo> builder)
    {
        builder.ToTable("Facilities");

        // Primary Key - uint stored as INTEGER
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        // Name - stored as string
        builder.Property(x => x.Name);

        // RequiredSpecialties - stored as comma-separated string
        builder.Property(x => x.RequiredSpecialties);

        // AcceptedMedicalBoards - stored as comma-separated string
        builder.Property(x => x.AcceptedMedicalBoards);

        // Bill rates - stored as decimal
        builder.Property(x => x.StandardBillRate);
        builder.Property(x => x.OvertimeBillRate);
    }
}
