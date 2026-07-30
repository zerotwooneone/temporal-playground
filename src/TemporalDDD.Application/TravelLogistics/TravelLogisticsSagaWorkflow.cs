using Temporalio.Workflows;

namespace TemporalDDD.Application.TravelLogistics;

[Workflow]
public class TravelLogisticsSagaWorkflow
{
    private string? _flightBookingId;
    private string? _lodgingBookingId;

    [WorkflowRun]
    public async Task RunAsync(TravelLogisticsInput input)
    {
        // Pass-through: No domain conversion needed - just pass primitives to activities
        try
        {
            // Step 1: Book Flight (External API)
            _flightBookingId = await Workflow.ExecuteActivityAsync(
                (ITravelLogisticsActivities activities) => activities.BookFlightAsync(new BookFlightInput(input.FlightNumber, input.OriginAirportCode, input.DestinationAirportCode, input.DepartureTimeUtc, input.FlightCostAmount)),
                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
            );

            // Step 2: Book Lodging (External API)
            _lodgingBookingId = await Workflow.ExecuteActivityAsync(
                (ITravelLogisticsActivities activities) => activities.BookLodgingAsync(new BookLodgingInput(input.HotelName, input.AddressStreet, input.AddressCity, input.AddressState, input.AddressZipCode, input.StayPeriodStartUtc, input.StayPeriodEndUtc, input.LodgingCostAmount)),
                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
            );

            // Step 3: Notify Traveler with confirmation
            var confirmationMessage = $"Your travel is booked! Flight: {input.FlightNumber} on {input.DepartureTimeUtc:yyyy-MM-dd}, Hotel: {input.HotelName} from {input.StayPeriodStartUtc:yyyy-MM-dd} to {input.StayPeriodEndUtc:yyyy-MM-dd}";
            await Workflow.ExecuteActivityAsync(
                (ITravelLogisticsActivities activities) => activities.NotifyTravelerAsync(new NotifyTravelerInput(input.TravelerEmail, confirmationMessage, false)),
                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
            );
        }
        catch (Exception ex)
        {
            // Compensating transaction: Cancel flight if lodging booking failed
            if (_flightBookingId is not null)
            {
                await Workflow.ExecuteActivityAsync(
                    (ITravelLogisticsActivities activities) => activities.CancelFlightAsync(new CancelFlightInput(_flightBookingId)),
                    new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
                );
            }

            // Compensating transaction: Cancel lodging if flight booking failed
            if (_lodgingBookingId is not null)
            {
                await Workflow.ExecuteActivityAsync(
                    (ITravelLogisticsActivities activities) => activities.CancelLodgingAsync(new CancelLodgingInput(_lodgingBookingId)),
                    new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
                );
            }

            // Notify traveler of cancellation
            var cancellationMessage = "Your travel booking could not be completed. All reservations have been cancelled.";
            await Workflow.ExecuteActivityAsync(
                (ITravelLogisticsActivities activities) => activities.NotifyTravelerAsync(new NotifyTravelerInput(input.TravelerEmail, cancellationMessage, true)),
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
