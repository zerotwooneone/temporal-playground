namespace TemporalDDD.Domain.TravelLogistics;

public interface ILodgingBookingRepository
{
    Task<LodgingBooking?> GetByIdAsync(LodgingBookingId id, CancellationToken cancellationToken = default);
    Task SaveAsync(LodgingBooking aggregate, CancellationToken cancellationToken = default);
}
