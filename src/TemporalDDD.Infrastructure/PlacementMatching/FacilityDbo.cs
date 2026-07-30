namespace TemporalDDD.Infrastructure.PlacementMatching;

public class FacilityDbo
{
    public uint Id { get; set; }
    public string Name { get; set; }
    public string RequiredSpecialties { get; set; }
    public string AcceptedMedicalBoards { get; set; }
    public decimal StandardBillRate { get; set; }
    public decimal OvertimeBillRate { get; set; }
}
