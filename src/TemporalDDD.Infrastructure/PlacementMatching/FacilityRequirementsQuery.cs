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
        var dbo = await _dbContext.Facilities
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == facilityId.ToString(), cancellationToken);

        if (dbo == null)
        {
            throw new InvalidOperationException($"Facility {facilityId} not found");
        }

        var specialties = dbo.RequiredSpecialties
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => Specialty.Create(s.Trim()).Value ?? throw new InvalidOperationException($"Invalid specialty: {s.Trim()}"))
            .ToList();

        var medicalBoards = dbo.AcceptedMedicalBoards
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(mb => MedicalBoard.Create(mb.Trim()).Value ?? throw new InvalidOperationException($"Invalid medical board: {mb.Trim()}"))
            .ToList();

        return new FacilityRequirementsDto(
            RequiredSpecialties: specialties,
            AcceptedMedicalBoards: medicalBoards
        );
    }
}
