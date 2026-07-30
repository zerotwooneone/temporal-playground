namespace TemporalDDD.Application.TimesheetProcessing;

/// <summary>
/// Primitive DTO for Timesheet Processing Workflow input.
/// All fields are primitive types to ensure clean JSON serialization with Temporal.
/// </summary>
public record TimesheetProcessingInput(
    string ProviderId,
    long PeriodStartUtc,
    long PeriodEndUtc,
    decimal TotalHours,
    decimal HourlyRate,
    decimal FacilityBillRateAmount,
    string FacilityBillRateCurrency
);
