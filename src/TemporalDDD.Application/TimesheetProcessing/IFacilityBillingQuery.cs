using TemporalDDD.Domain.PlacementMatching;

namespace TemporalDDD.Application.TimesheetProcessing;

public interface IFacilityBillingQuery
{
    Task<FacilityBillingRateDto> GetFacilityBillingRateAsync(FacilityId facilityId, AssignmentId assignmentId, CancellationToken cancellationToken = default);
}

public record FacilityBillingRateDto(decimal StandardBillRate, decimal OvertimeBillRate, string Currency);
