using Temporalio.Activities;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.TimesheetProcessing;
using TemporalDDD.Domain.TimesheetProcessing.ValueObjects;

namespace TemporalDDD.Application.TimesheetProcessing;

public interface ITimesheetProcessingActivities
{
    Task ValidateTimesheetRulesAsync(TimesheetId timesheetId);
    Task<PayrollCalculationResult> CalculatePayrollAndTaxesAsync(TimesheetId timesheetId);
    Task<string> SubmitBankTransferAsync(TimesheetId timesheetId, string idempotencyKey);
    Task<string> GenerateAndSendInvoiceAsync(TimesheetId timesheetId, Money facilityBillRate);
}

public record PayrollCalculationResult(
    Money GrossPay,
    Money TaxAmount,
    Money NetPay
);
