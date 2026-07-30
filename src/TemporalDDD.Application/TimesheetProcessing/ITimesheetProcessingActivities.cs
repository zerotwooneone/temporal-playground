using Temporalio.Activities;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.TimesheetProcessing;
using TemporalDDD.Domain.TimesheetProcessing.ValueObjects;

namespace TemporalDDD.Application.TimesheetProcessing;

public interface ITimesheetProcessingActivities
{
    [Activity]
    Task ValidateTimesheetRulesAsync(ValidateTimesheetInput input);
    [Activity]
    Task<PayrollCalculationResult> CalculatePayrollAndTaxesAsync(CalculatePayrollInput input);
    [Activity]
    Task<string> SubmitBankTransferAsync(SubmitBankTransferInput input);
    [Activity]
    Task<string> GenerateAndSendInvoiceAsync(GenerateInvoiceInput input);
}

// Primitive DTOs for activity parameters
public record ValidateTimesheetInput(uint TimesheetId);
public record CalculatePayrollInput(uint TimesheetId);
public record SubmitBankTransferInput(uint TimesheetId, string IdempotencyKey);
public record GenerateInvoiceInput(uint TimesheetId, decimal FacilityBillRate);

// Domain result types
public record PayrollCalculationResult(
    Money GrossPay,
    Money TaxAmount,
    Money NetPay
);
