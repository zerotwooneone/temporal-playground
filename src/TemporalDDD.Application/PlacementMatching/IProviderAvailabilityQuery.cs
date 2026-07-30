using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.PlacementMatching;

namespace TemporalDDD.Application.PlacementMatching;

public interface IProviderAvailabilityQuery
{
    Task<ProviderAvailabilityDto> GetProviderAvailabilityAsync(ProviderId providerId, DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default);
}

public record ProviderAvailabilityDto(bool IsAvailable, AssignmentPublicId? ConflictingAssignmentId);
