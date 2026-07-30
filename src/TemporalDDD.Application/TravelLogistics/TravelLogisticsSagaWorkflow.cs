using Temporalio.Workflows;
using TemporalDDD.Application.TravelLogistics;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.TravelLogistics;
using TemporalDDD.Domain.TravelLogistics.ValueObjects;

namespace TemporalDDD.Application.TravelLogistics;

[Workflow]
public class TravelLogisticsSagaWorkflow
{
    private FlightBookingId? _flightBookingId;
    private LodgingBookingId? _lodgingBookingId;

    [WorkflowRun]
    public async Task RunAsync(
        Email travelerEmail,
        FlightNumber flightNumber, 
        AirportCode origin, 
        AirportCode destination, 
        FlightDepartureTime departureTime, 
        Money flightCost,
        HotelName hotelName,
        Address address,
        DateRange stayPeriod,
        Money lodgingCost)
    {
        try
        {
            // Step 1: Book Flight (External API)
            _flightBookingId = await Workflow.ExecuteActivityAsync(
                (ITravelLogisticsActivities activities) => activities.BookFlightAsync(flightNumber, origin, destination, departureTime, flightCost),
                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
            );

            // Step 2: Book Lodging (External API)
            _lodgingBookingId = await Workflow.ExecuteActivityAsync(
                (ITravelLogisticsActivities activities) => activities.BookLodgingAsync(hotelName, address, stayPeriod, lodgingCost),
                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
            );

            // Step 3: Notify Traveler with confirmation
            var confirmationMessage = $"Your travel is booked! Flight: {flightNumber.Value} on {departureTime.Value:yyyy-MM-dd}, Hotel: {hotelName.Value} from {stayPeriod.Start:yyyy-MM-dd} to {stayPeriod.End:yyyy-MM-dd}";
            await Workflow.ExecuteActivityAsync(
                (ITravelLogisticsActivities activities) => activities.NotifyTravelerAsync(travelerEmail, confirmationMessage, isCancellation: false),
                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
            );
        }
        catch (Exception ex)
        {
            // Compensating transaction: Cancel flight if lodging booking failed
            if (_flightBookingId is not null)
            {
                await Workflow.ExecuteActivityAsync(
                    (ITravelLogisticsActivities activities) => activities.CancelFlightAsync(_flightBookingId),
                    new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
                );
            }

            // Compensating transaction: Cancel lodging if flight booking failed
            if (_lodgingBookingId is not null)
            {
                await Workflow.ExecuteActivityAsync(
                    (ITravelLogisticsActivities activities) => activities.CancelLodgingAsync(_lodgingBookingId),
                    new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
                );
            }

            // Notify traveler of cancellation
            var cancellationMessage = "Your travel booking could not be completed. All reservations have been cancelled.";
            await Workflow.ExecuteActivityAsync(
                (ITravelLogisticsActivities activities) => activities.NotifyTravelerAsync(travelerEmail, cancellationMessage, isCancellation: true),
                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
            );

            // Re-throw the exception to mark workflow as failed
            throw new TravelLogisticsException("Travel booking failed. All reservations cancelled.", ex);
        }
    }
}

public class TravelLogisticsException : Exception
{
    public TravelLogisticsException(string message, Exception innerException) : base(message, innerException) { }
}
