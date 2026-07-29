using Microsoft.EntityFrameworkCore;
using Temporalio.Activities;
using TemporalDDD.Application.TimesheetProcessing;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.TimesheetProcessing;

public class TimesheetProcessingActivities : ITimesheetProcessingActivities
{
    private readonly ApplicationDbContext _dbContext;

    public TimesheetProcessingActivities(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ValidateTimesheetRulesAsync(Guid timesheetId)
    {
        // Simulate database operation to load and validate timesheet
        await _dbContext.Database.EnsureCreatedAsync();

        // Create domain entity for validation
        var timesheet = new Domain.TimesheetProcessing.Timesheet(
            providerId: Guid.NewGuid(),
            periodStart: DateTime.UtcNow.AddDays(-30),
            periodEnd: DateTime.UtcNow,
            totalHours: 160,
            hourlyRate: 50
        );

        // Validate business rules
        timesheet.Validate();

        // Note: DbSet would be added to ApplicationDbContext once schema is defined
        // For now, we simulate the save
        await Task.Delay(100);

        Console.WriteLine($"[TimesheetValidation] Timesheet {timesheetId} validated successfully");
    }

    public async Task<PayrollCalculationResult> CalculatePayrollAndTaxesAsync(Guid timesheetId)
    {
        // Simulate database operation to load timesheet and calculate payroll
        await _dbContext.Database.EnsureCreatedAsync();

        // In real implementation, this would load the timesheet from database
        var timesheet = new Domain.TimesheetProcessing.Timesheet(
            providerId: Guid.NewGuid(),
            periodStart: DateTime.UtcNow.AddDays(-30),
            periodEnd: DateTime.UtcNow,
            totalHours: 160,
            hourlyRate: 50
        );

        // Calculate payroll with tax rate (e.g., 25%)
        const decimal taxRate = 0.25m;
        timesheet.CalculatePayroll(taxRate);

        await Task.Delay(100);

        Console.WriteLine($"[PayrollCalculation] Gross: {timesheet.GrossPay:C}, Tax: {timesheet.TaxAmount:C}, Net: {timesheet.NetPay:C}");

        return new PayrollCalculationResult(
            GrossPay: timesheet.GrossPay,
            TaxAmount: timesheet.TaxAmount,
            NetPay: timesheet.NetPay
        );
    }

    public async Task<string> SubmitBankTransferAsync(Guid timesheetId, string idempotencyKey)
    {
        // Simulate external API call to payment gateway with idempotency key
        await Task.Delay(2000);

        // In real implementation, this would call the bank transfer API
        // The idempotencyKey ensures that duplicate requests don't result in duplicate payments
        var paymentReference = $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}-{idempotencyKey.Substring(0, 8)}";

        Console.WriteLine($"[BankTransfer] Submitted transfer for timesheet {timesheetId} with idempotency key: {idempotencyKey}");
        Console.WriteLine($"[BankTransfer] Payment reference: {paymentReference}");

        return paymentReference;
    }

    public async Task<string> GenerateAndSendInvoiceAsync(Guid timesheetId, decimal facilityBillRate)
    {
        // Simulate database operation to load timesheet and calculate facility bill
        await _dbContext.Database.EnsureCreatedAsync();

        // In real implementation, this would load the timesheet from database
        var timesheet = new Domain.TimesheetProcessing.Timesheet(
            providerId: Guid.NewGuid(),
            periodStart: DateTime.UtcNow.AddDays(-30),
            periodEnd: DateTime.UtcNow,
            totalHours: 160,
            hourlyRate: 50
        );

        // Calculate facility bill (typically higher than provider pay rate)
        var facilityBillAmount = timesheet.TotalHours * facilityBillRate;

        // Simulate external API call to ERP system to send invoice
        await Task.Delay(1000);

        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

        Console.WriteLine($"[InvoiceGeneration] Generated invoice {invoiceNumber} for timesheet {timesheetId}");
        Console.WriteLine($"[InvoiceGeneration] Facility bill amount: {facilityBillAmount:C} (Rate: {facilityBillRate:C}/hr)");

        return invoiceNumber;
    }
}
