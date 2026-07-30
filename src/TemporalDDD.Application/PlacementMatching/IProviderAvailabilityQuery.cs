using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.PlacementMatching;

namespace TemporalDDD.Application.PlacementMatching;

public interface IProviderAvailabilityQuery
{
    Task<ProviderAvailabilityDto> GetProviderAvailabilityAsync(ProviderId providerId, DateRange period, CancellationToken cancellationToken = default);
}

public record ProviderAvailabilityDto(bool IsAvailable, string? ConflictingAssignmentId);
