using TemporalDDD.Domain.PlacementMatching;

namespace TemporalDDD.Application.PlacementMatching;

public interface IFacilityRequirementsQuery
{
    Task<FacilityRequirementsDto> GetFacilityRequirementsAsync(FacilityId facilityId, CancellationToken cancellationToken = default);
}

public record FacilityRequirementsDto(IReadOnlyList<string> RequiredSpecialties, IReadOnlyList<string> AcceptedMedicalBoards);
