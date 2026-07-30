using Microsoft.EntityFrameworkCore;
using TemporalDDD.Application.PlacementMatching;
using TemporalDDD.Domain.PlacementMatching;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.PlacementMatching;

public class FacilityRequirementsQuery : IFacilityRequirementsQuery
{
    private readonly ApplicationDbContext _dbContext;

    public FacilityRequirementsQuery(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FacilityRequirementsDto> GetFacilityRequirementsAsync(FacilityId facilityId, CancellationToken cancellationToken = default)
    {
        var facility = await _dbContext.Facilities
            .FirstOrDefaultAsync(f => f.Id == facilityId.Value, cancellationToken);

        if (facility == null)
        {
            throw new InvalidOperationException($"Facility {facilityId} not found");
        }

        var specialties = facility.RequiredSpecialties
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => Specialty.Create(s.Trim()))
            .ToList();

        var medicalBoards = facility.AcceptedMedicalBoards
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(mb => MedicalBoard.Create(mb.Trim()).Value)
            .ToList();

        return new FacilityRequirementsDto(
            RequiredSpecialties: specialties,
            AcceptedMedicalBoards: medicalBoards
        );
    }
}
