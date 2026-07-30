using Temporalio.Activities;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.TimesheetProcessing;
using TemporalDDD.Domain.TimesheetProcessing.ValueObjects;

namespace TemporalDDD.Application.TimesheetProcessing;

public interface ITimesheetProcessingActivities
{
    [Activity]
    Task ValidateTimesheetRulesAsync(TimesheetId timesheetId);
    [Activity]
    Task<PayrollCalculationResult> CalculatePayrollAndTaxesAsync(TimesheetId timesheetId);
    [Activity]
    Task<string> SubmitBankTransferAsync(TimesheetId timesheetId, string idempotencyKey);
    [Activity]
    Task<string> GenerateAndSendInvoiceAsync(TimesheetId timesheetId, Money facilityBillRate);
}

public record PayrollCalculationResult(
    Money GrossPay,
    Money TaxAmount,
    Money NetPay
);
