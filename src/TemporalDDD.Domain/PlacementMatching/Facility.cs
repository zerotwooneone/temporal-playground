using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.PlacementMatching;

public sealed class Facility
{
    public FacilityId Id { get; private set; }
    public string Name { get; private set; }
    public string RequiredSpecialties { get; private set; }
    public string AcceptedMedicalBoards { get; private set; }
    public decimal StandardBillRate { get; private set; }
    public decimal OvertimeBillRate { get; private set; }

    private Facility() { }

    public static Facility Create(string name, string requiredSpecialties, string acceptedMedicalBoards, decimal standardBillRate, decimal overtimeBillRate)
    {
        return new Facility
        {
            Id = FacilityId.Create(0),
            Name = name,
            RequiredSpecialties = requiredSpecialties,
            AcceptedMedicalBoards = acceptedMedicalBoards,
            StandardBillRate = standardBillRate,
            OvertimeBillRate = overtimeBillRate
        };
    }
}
