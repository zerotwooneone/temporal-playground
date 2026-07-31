using Microsoft.EntityFrameworkCore;
using TemporalDDD.Application.TimesheetProcessing;
using TemporalDDD.Domain.PlacementMatching;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.TimesheetProcessing;

public class FacilityBillingQuery : IFacilityBillingQuery
{
    private readonly ApplicationDbContext _dbContext;

    public FacilityBillingQuery(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FacilityBillingRateDto> GetFacilityBillingRateAsync(FacilityId facilityId, AssignmentId assignmentId, CancellationToken cancellationToken = default)
    {
        var dbo = await _dbContext.Facilities
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == facilityId.ToString(), cancellationToken);

        if (dbo == null)
        {
            throw new InvalidOperationException($"Facility {facilityId} not found");
        }

        return new FacilityBillingRateDto(
            StandardBillRate: Money.Create(dbo.StandardBillRate).Value ?? throw new InvalidOperationException("Failed to create standard bill rate"),
            OvertimeBillRate: Money.Create(dbo.OvertimeBillRate).Value ?? throw new InvalidOperationException("Failed to create overtime bill rate")
        );
    }
}
