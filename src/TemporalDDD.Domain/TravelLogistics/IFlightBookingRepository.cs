namespace TemporalDDD.Domain.TravelLogistics;

public interface IFlightBookingRepository
{
    Task<FlightBooking?> GetByIdAsync(FlightBookingId id, CancellationToken cancellationToken = default);
    Task SaveAsync(FlightBooking aggregate, CancellationToken cancellationToken = default);
}
