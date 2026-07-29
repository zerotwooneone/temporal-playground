using Temporalio.Workflows;
using TemporalDDD.Application.TravelLogistics;

namespace TemporalDDD.Application.TravelLogistics;

[Workflow]
public class TravelLogisticsSagaWorkflow
{
    private uint? _flightBookingId;
    private uint? _lodgingBookingId;

    [WorkflowRun]
    public async Task RunAsync(
        string travelerEmail,
        string flightNumber, 
        string origin, 
        string destination, 
        DateTime departureTime, 
        decimal flightCost,
        string hotelName,
        string address,
        DateTime checkInDate,
        DateTime checkOutDate,
        decimal lodgingCost)
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
                (ITravelLogisticsActivities activities) => activities.BookLodgingAsync(hotelName, address, checkInDate, checkOutDate, lodgingCost),
                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
            );

            // Step 3: Notify Traveler with confirmation
            var confirmationMessage = $"Your travel is booked! Flight: {flightNumber} on {departureTime:yyyy-MM-dd}, Hotel: {hotelName} from {checkInDate:yyyy-MM-dd} to {checkOutDate:yyyy-MM-dd}";
            await Workflow.ExecuteActivityAsync(
                (ITravelLogisticsActivities activities) => activities.NotifyTravelerAsync(travelerEmail, confirmationMessage, isCancellation: false),
                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
            );
        }
        catch (Exception ex)
        {
            // Compensating transaction: Cancel flight if lodging booking failed
            if (_flightBookingId.HasValue)
            {
                await Workflow.ExecuteActivityAsync(
                    (ITravelLogisticsActivities activities) => activities.CancelFlightAsync(_flightBookingId.Value),
                    new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
                );
            }

            // Compensating transaction: Cancel lodging if flight booking failed
            if (_lodgingBookingId.HasValue)
            {
                await Workflow.ExecuteActivityAsync(
                    (ITravelLogisticsActivities activities) => activities.CancelLodgingAsync(_lodgingBookingId.Value),
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
