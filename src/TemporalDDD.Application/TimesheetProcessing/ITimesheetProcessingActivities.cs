using Temporalio.Activities;

namespace TemporalDDD.Application.TimesheetProcessing;

public interface ITimesheetProcessingActivities
{
    Task ValidateTimesheetRulesAsync(uint timesheetId);
    Task<PayrollCalculationResult> CalculatePayrollAndTaxesAsync(uint timesheetId);
    Task<string> SubmitBankTransferAsync(uint timesheetId, string idempotencyKey);
    Task<string> GenerateAndSendInvoiceAsync(uint timesheetId, decimal facilityBillRate);
}

public record PayrollCalculationResult(
    decimal GrossPay,
    decimal TaxAmount,
    decimal NetPay
);
