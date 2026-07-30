namespace TemporalDDD.Application.PlacementMatching;

/// <summary>
/// Primitive DTO for Placement Matching Workflow input.
/// All fields are primitive types to ensure clean JSON serialization with Temporal.
/// </summary>
public record PlacementMatchingInput(
    uint ProviderId,
    uint FacilityId,
    uint PositionId
);
