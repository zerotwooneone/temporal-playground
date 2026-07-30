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
            .FirstOrDefaultAsync(f => f.Id == facilityId.Value, cancellationToken);

        if (dbo == null)
        {
            throw new InvalidOperationException($"Facility {facilityId} not found");
        }

        return new FacilityBillingRateDto(
            StandardBillRate: Money.Create(dbo.StandardBillRate),
            OvertimeBillRate: Money.Create(dbo.OvertimeBillRate)
        );
    }
}
