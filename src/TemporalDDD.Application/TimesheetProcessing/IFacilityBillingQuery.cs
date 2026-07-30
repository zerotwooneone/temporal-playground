using TemporalDDD.Domain.PlacementMatching;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Application.TimesheetProcessing;

public interface IFacilityBillingQuery
{
    Task<FacilityBillingRateDto> GetFacilityBillingRateAsync(FacilityId facilityId, AssignmentId assignmentId, CancellationToken cancellationToken = default);
}

public record FacilityBillingRateDto(Money StandardBillRate, Money OvertimeBillRate);
