using TemporalDDD.Domain.PlacementMatching;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;

namespace TemporalDDD.Application.PlacementMatching;

public interface IFacilityRequirementsQuery
{
    Task<FacilityRequirementsDto> GetFacilityRequirementsAsync(FacilityId facilityId, CancellationToken cancellationToken = default);
}

public record FacilityRequirementsDto(IReadOnlyList<Specialty> RequiredSpecialties, IReadOnlyList<MedicalBoard> AcceptedMedicalBoards);
