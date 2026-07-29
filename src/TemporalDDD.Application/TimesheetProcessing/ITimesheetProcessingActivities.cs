using Temporalio.Activities;

namespace TemporalDDD.Application.TimesheetProcessing;

public interface ITimesheetProcessingActivities
{
    Task ValidateTimesheetRulesAsync(Guid timesheetId);
    Task<PayrollCalculationResult> CalculatePayrollAndTaxesAsync(Guid timesheetId);
    Task<string> SubmitBankTransferAsync(Guid timesheetId, string idempotencyKey);
    Task<string> GenerateAndSendInvoiceAsync(Guid timesheetId, decimal facilityBillRate);
}

public record PayrollCalculationResult(
    decimal GrossPay,
    decimal TaxAmount,
    decimal NetPay
);
