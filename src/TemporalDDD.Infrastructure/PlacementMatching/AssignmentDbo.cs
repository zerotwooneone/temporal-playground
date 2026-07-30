namespace TemporalDDD.Infrastructure.PlacementMatching;

public class AssignmentDbo
{
    public string Id { get; set; }
    public string? PublicId { get; set; }
    public string ProviderId { get; set; }
    public string FacilityId { get; set; }
    public string PositionId { get; set; }
    public decimal MatchScore { get; set; }
    public int Status { get; set; }
    public long ProposedAt { get; set; } // Unix milliseconds
    public long? AcceptedAt { get; set; } // Unix milliseconds
    public int Version { get; set; }
}
