using Temporalio.Activities;

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
public record ValidateTimesheetInput(string TimesheetId);
public record CalculatePayrollInput(string TimesheetId);
public record SubmitBankTransferInput(string TimesheetId, string IdempotencyKey);
public record GenerateInvoiceInput(string TimesheetId, decimal FacilityBillRate);

// Primitive result types
public record PayrollCalculationResult(
    decimal GrossPayAmount,
    string GrossPayCurrency,
    decimal TaxAmount,
    string TaxCurrency,
    decimal NetPayAmount,
    string NetPayCurrency
);
