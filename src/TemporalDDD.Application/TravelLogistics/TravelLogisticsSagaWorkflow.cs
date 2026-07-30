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
    public async Task RunAsync(TravelLogisticsInput input)
    {
        // Elevate to Domain types with catastrophic assertions
        var travelerEmailResult = Email.Create(input.TravelerEmail);
        if (travelerEmailResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid Email. {travelerEmailResult.Error}");

        var flightNumberResult = FlightNumber.Create(input.FlightNumber);
        if (flightNumberResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid FlightNumber. {flightNumberResult.Error}");

        var originResult = AirportCode.Create(input.OriginAirportCode);
        if (originResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid OriginAirportCode. {originResult.Error}");

        var destinationResult = AirportCode.Create(input.DestinationAirportCode);
        if (destinationResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid DestinationAirportCode. {destinationResult.Error}");

        var departureTime = DateTimeOffset.FromUnixTimeMilliseconds(input.DepartureTimeUtc);
        var departureTimeResult = FlightDepartureTime.Create(departureTime);
        if (departureTimeResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid FlightDepartureTime. {departureTimeResult.Error}");

        var flightCostResult = Money.Create(input.FlightCostAmount, input.FlightCostCurrency);
        if (flightCostResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid FlightCost. {flightCostResult.Error}");

        var hotelNameResult = HotelName.Create(input.HotelName);
        if (hotelNameResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid HotelName. {hotelNameResult.Error}");

        var addressResult = Address.Create(input.AddressStreet, input.AddressCity, input.AddressState, input.AddressZipCode);
        if (addressResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid Address. {addressResult.Error}");

        var stayPeriodStart = DateTimeOffset.FromUnixTimeMilliseconds(input.StayPeriodStartUtc);
        var stayPeriodEnd = DateTimeOffset.FromUnixTimeMilliseconds(input.StayPeriodEndUtc);
        var stayPeriodResult = DateRange.Create(stayPeriodStart, stayPeriodEnd);
        if (stayPeriodResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid DateRange. {stayPeriodResult.Error}");

        var lodgingCostResult = Money.Create(input.LodgingCostAmount, input.LodgingCostCurrency);
        if (lodgingCostResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid LodgingCost. {lodgingCostResult.Error}");

        var travelerEmail = travelerEmailResult.Value;
        var flightNumber = flightNumberResult.Value;
        var origin = originResult.Value;
        var destination = destinationResult.Value;
        var departureTimeValue = departureTimeResult.Value;
        var flightCost = flightCostResult.Value;
        var hotelName = hotelNameResult.Value;
        var address = addressResult.Value;
        var stayPeriod = stayPeriodResult.Value;
        var lodgingCost = lodgingCostResult.Value;
        try
        {
            // Step 1: Book Flight (External API)
            _flightBookingId = await Workflow.ExecuteActivityAsync(
                (ITravelLogisticsActivities activities) => activities.BookFlightAsync(new BookFlightInput(flightNumber.Value, origin.Value, destination.Value, departureTimeValue.Value, flightCost.Amount)),
                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
            );

            // Step 2: Book Lodging (External API)
            _lodgingBookingId = await Workflow.ExecuteActivityAsync(
                (ITravelLogisticsActivities activities) => activities.BookLodgingAsync(new BookLodgingInput(hotelName.Value, address.Street, address.City, address.State, address.ZipCode, stayPeriod.Start, stayPeriod.End, lodgingCost.Amount)),
                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
            );

            // Step 3: Notify Traveler with confirmation
            var confirmationMessage = $"Your travel is booked! Flight: {flightNumber.Value} on {departureTimeValue.Value:yyyy-MM-dd}, Hotel: {hotelName.Value} from {stayPeriod.Start:yyyy-MM-dd} to {stayPeriod.End:yyyy-MM-dd}";
            await Workflow.ExecuteActivityAsync(
                (ITravelLogisticsActivities activities) => activities.NotifyTravelerAsync(new NotifyTravelerInput(travelerEmail.Value, confirmationMessage, false)),
                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
            );
        }
        catch (Exception ex)
        {
            // Compensating transaction: Cancel flight if lodging booking failed
            if (_flightBookingId is not null)
            {
                await Workflow.ExecuteActivityAsync(
                    (ITravelLogisticsActivities activities) => activities.CancelFlightAsync(new CancelFlightInput(_flightBookingId.Value)),
                    new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
                );
            }

            // Compensating transaction: Cancel lodging if flight booking failed
            if (_lodgingBookingId is not null)
            {
                await Workflow.ExecuteActivityAsync(
                    (ITravelLogisticsActivities activities) => activities.CancelLodgingAsync(new CancelLodgingInput(_lodgingBookingId.Value)),
                    new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
                );
            }

            // Notify traveler of cancellation
            var cancellationMessage = "Your travel booking could not be completed. All reservations have been cancelled.";
            await Workflow.ExecuteActivityAsync(
                (ITravelLogisticsActivities activities) => activities.NotifyTravelerAsync(new NotifyTravelerInput(travelerEmail.Value, cancellationMessage, true)),
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
