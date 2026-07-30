namespace TemporalDDD.Infrastructure.PlacementMatching;

public class AssignmentDbo
{
    public uint Id { get; set; }
    public string? PublicId { get; set; }
    public uint ProviderId { get; set; }
    public uint FacilityId { get; set; }
    public uint PositionId { get; set; }
    public decimal MatchScore { get; set; }
    public int Status { get; set; }
    public long ProposedAt { get; set; } // Unix milliseconds
    public long? AcceptedAt { get; set; } // Unix milliseconds
    public int Version { get; set; }
}
