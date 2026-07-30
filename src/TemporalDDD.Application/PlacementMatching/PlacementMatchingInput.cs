namespace TemporalDDD.Application.PlacementMatching;

/// <summary>
/// Primitive DTO for Placement Matching Workflow input.
/// All fields are primitive types to ensure clean JSON serialization with Temporal.
/// </summary>
public record PlacementMatchingInput(
    string ProviderId,
    string FacilityId,
    string PositionId
);
