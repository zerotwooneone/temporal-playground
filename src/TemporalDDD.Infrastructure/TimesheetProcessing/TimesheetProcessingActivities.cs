using Microsoft.EntityFrameworkCore;
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

    public async Task ValidateTimesheetRulesAsync(uint timesheetId)
    {
        var timesheetIdVo = TimesheetId.Create(timesheetId);
        var timesheet = await _timesheetRepository.GetByIdAsync(timesheetIdVo);

        if (timesheet == null)
        {
            // Create value objects for new timesheet
            var periodVo = DateRange.Create(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
            var totalHoursVo = Hours.Create(160);
            var hourlyRateVo = HourlyRate.Create(50);
            var providerIdVo = ProviderId.Create(1); // Simulated provider ID

            // Create domain entity for validation using factory
            timesheet = Domain.TimesheetProcessing.Timesheet.Create(
                providerId: providerIdVo,
                period: periodVo,
                totalHours: totalHoursVo,
                hourlyRate: hourlyRateVo
            );
        }

        // Validate business rules
        timesheet.Validate();
        await _timesheetRepository.SaveAsync(timesheet);

        Console.WriteLine($"[TimesheetValidation] Timesheet {timesheetId} validated successfully");
    }

    public async Task<PayrollCalculationResult> CalculatePayrollAndTaxesAsync(uint timesheetId)
    {
        var timesheetIdVo = TimesheetId.Create(timesheetId);
        var timesheet = await _timesheetRepository.GetByIdAsync(timesheetIdVo);

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
            GrossPay: timesheet.GrossPay.Amount,
            TaxAmount: timesheet.TaxAmount.Amount,
            NetPay: timesheet.NetPay.Amount
        );
    }

    public async Task<string> SubmitBankTransferAsync(uint timesheetId, string idempotencyKey)
    {
        // Simulate external API call to payment gateway with chaos (100ms latency, 10% failure rate)
        _chaosHttpClient
            .WithLatency(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
            .WithFailureRate(0.10, System.Net.HttpStatusCode.InternalServerError);

        var response = await _chaosHttpClient.PostAsJsonAsync($"/api/payments/transfer", new { TimesheetId = timesheetId, IdempotencyKey = idempotencyKey });

        // The idempotencyKey ensures that duplicate requests don't result in duplicate payments
        var paymentReference = $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}-{idempotencyKey.Substring(0, 8)}";

        Console.WriteLine($"[BankTransfer] Submitted transfer for timesheet {timesheetId} with idempotency key: {idempotencyKey}");
        Console.WriteLine($"[BankTransfer] Payment reference: {paymentReference}");

        return paymentReference;
    }

    public async Task<string> GenerateAndSendInvoiceAsync(uint timesheetId, decimal facilityBillRate)
    {
        var timesheetIdVo = TimesheetId.Create(timesheetId);
        var timesheet = await _timesheetRepository.GetByIdAsync(timesheetIdVo);

        if (timesheet == null)
        {
            throw new InvalidOperationException($"Timesheet {timesheetId} not found");
        }

        // Calculate facility bill (typically higher than provider pay rate)
        var facilityBillAmount = timesheet.TotalHours.Value * facilityBillRate;

        // Simulate external API call to ERP system with chaos (100ms latency, 10% failure rate)
        _chaosHttpClient
            .WithLatency(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
            .WithFailureRate(0.10, System.Net.HttpStatusCode.InternalServerError);

        var response = await _chaosHttpClient.PostAsJsonAsync($"/api/erp/invoices", new { TimesheetId = timesheetId, FacilityBillRate = facilityBillRate });

        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

        Console.WriteLine($"[InvoiceGeneration] Generated invoice {invoiceNumber} for timesheet {timesheetId}");
        Console.WriteLine($"[InvoiceGeneration] Facility bill amount: {facilityBillAmount:C} (Rate: {facilityBillRate:C}/hr)");

        return invoiceNumber;
    }
}
