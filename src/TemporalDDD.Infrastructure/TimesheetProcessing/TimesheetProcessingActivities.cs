using Temporalio.Activities;
using TemporalDDD.Application.TimesheetProcessing;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.TimesheetProcessing;
using TemporalDDD.Domain.TimesheetProcessing.ValueObjects;
using TemporalDDD.Infrastructure.Testing;

namespace TemporalDDD.Infrastructure.TimesheetProcessing;

public class TimesheetProcessingActivities : ITimesheetProcessingActivities
{
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly ChaosHttpClient _chaosHttpClient;

    public TimesheetProcessingActivities(ITimesheetRepository timesheetRepository, ChaosHttpClient chaosHttpClient)
    {
        _timesheetRepository = timesheetRepository;
        _chaosHttpClient = chaosHttpClient;
    }

    [Activity]
    public async Task ValidateTimesheetRulesAsync(ValidateTimesheetInput input)
    {
        // Convert primitive DTO to Domain types with fail-fast validation
        var timesheetIdResult = TimesheetId.Create(input.TimesheetId);
        if (timesheetIdResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid TimesheetId. {timesheetIdResult.Error}");

        var timesheetId = timesheetIdResult.Value;
        var timesheet = await _timesheetRepository.GetByIdAsync(timesheetId);

        if (timesheet == null)
        {
            // Create value objects for new timesheet using Result<T>
            var period = DateRange.Create(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow).Value!;
            var totalHours = Hours.Create(160).Value!;
            var hourlyRate = HourlyRate.Create(50).Value!;
            var providerIdResult = ProviderId.New(); // Simulated provider ID

            // Create domain entity for validation using factory
            timesheet = Domain.TimesheetProcessing.Timesheet.Create(
                providerId: providerIdResult,
                period: period,
                totalHours: totalHours,
                hourlyRate: hourlyRate
            );
        }

        // Validate business rules
        timesheet.Validate();
        await _timesheetRepository.SaveAsync(timesheet);

        Console.WriteLine($"[TimesheetValidation] Timesheet {timesheetId} validated successfully");
    }

    [Activity]
    public async Task<PayrollCalculationResult> CalculatePayrollAndTaxesAsync(CalculatePayrollInput input)
    {
        // Convert primitive DTO to Domain types with fail-fast validation
        var timesheetIdResult = TimesheetId.Create(input.TimesheetId);
        if (timesheetIdResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid TimesheetId. {timesheetIdResult.Error}");

        var timesheetId = timesheetIdResult.Value;
        var timesheet = await _timesheetRepository.GetByIdAsync(timesheetId);

        if (timesheet == null)
        {
            throw new InvalidOperationException($"Timesheet {timesheetId} not found");
        }

        // Calculate payroll with tax rate (e.g., 25%)
        const decimal taxRate = 0.25m;
        timesheet.CalculatePayroll(taxRate);
        await _timesheetRepository.SaveAsync(timesheet);

        Console.WriteLine($"[PayrollCalculation] Gross: {timesheet.GrossPay}, Tax: {timesheet.TaxAmount}, Net: {timesheet.NetPay}");

        return new PayrollCalculationResult(
            GrossPayAmount: timesheet.GrossPay.Amount,
            GrossPayCurrency: timesheet.GrossPay.Currency,
            TaxAmount: timesheet.TaxAmount.Amount,
            TaxCurrency: timesheet.TaxAmount.Currency,
            NetPayAmount: timesheet.NetPay.Amount,
            NetPayCurrency: timesheet.NetPay.Currency
        );
    }

    [Activity]
    public async Task<string> SubmitBankTransferAsync(SubmitBankTransferInput input)
    {
        // Convert primitive DTO to Domain types with fail-fast validation
        var timesheetIdResult = TimesheetId.Create(input.TimesheetId);
        if (timesheetIdResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid TimesheetId. {timesheetIdResult.Error}");

        var timesheetId = timesheetIdResult.Value;

        // Simulate external API call to payment gateway with chaos (100ms latency, 10% failure rate)
        var response = await _chaosHttpClient
            .WithLatency(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
            .WithFailureRate(0.10, System.Net.HttpStatusCode.InternalServerError)
            .PostAsJsonAsync("https://payment-gateway.example.com/api/transfer", new { TimesheetId = timesheetId, IdempotencyKey = input.IdempotencyKey });

        // The idempotencyKey ensures that duplicate requests don't result in duplicate payments
        var paymentReference = $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}-{input.IdempotencyKey.Substring(0, 8)}";

        Console.WriteLine($"[BankTransfer] Submitted transfer for timesheet {timesheetId} with idempotency key: {input.IdempotencyKey}");
        Console.WriteLine($"[BankTransfer] Payment reference: {paymentReference}");

        return paymentReference;
    }

    [Activity]
    public async Task<string> GenerateAndSendInvoiceAsync(GenerateInvoiceInput input)
    {
        // Convert primitive DTO to Domain types with fail-fast validation
        var timesheetIdResult = TimesheetId.Create(input.TimesheetId);
        if (timesheetIdResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid TimesheetId. {timesheetIdResult.Error}");
        
        var facilityBillRateResult = Money.Create(input.FacilityBillRate);
        if (facilityBillRateResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid FacilityBillRate. {facilityBillRateResult.Error}");

        var timesheetId = timesheetIdResult.Value;
        var facilityBillRate = facilityBillRateResult.Value;

        var timesheet = await _timesheetRepository.GetByIdAsync(timesheetId);

        if (timesheet == null)
        {
            throw new InvalidOperationException($"Timesheet {timesheetId} not found");
        }

        // Calculate facility bill (typically higher than provider pay rate)
        var facilityBillAmount = Money.Create(timesheet.TotalHours.Value * facilityBillRate.Amount);

        // Simulate external API call to ERP system with chaos (100ms latency, 10% failure rate)
        var response = await _chaosHttpClient
            .WithLatency(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
            .WithFailureRate(0.10, System.Net.HttpStatusCode.InternalServerError)
            .PostAsJsonAsync("https://erp-system.example.com/api/invoices", new { TimesheetId = timesheetId, FacilityBillRate = facilityBillRate.Amount });

        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

        Console.WriteLine($"[InvoiceGeneration] Generated invoice {invoiceNumber} for timesheet {timesheetId}");
        Console.WriteLine($"[InvoiceGeneration] Facility bill amount: {facilityBillAmount} (Rate: {facilityBillRate}/hr)");

        return invoiceNumber;
    }
}
