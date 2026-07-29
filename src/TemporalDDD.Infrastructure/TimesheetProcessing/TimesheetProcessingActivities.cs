using Microsoft.EntityFrameworkCore;
using Temporalio.Activities;
using TemporalDDD.Application.TimesheetProcessing;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.TimesheetProcessing.ValueObjects;
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

        // Create value objects
        var periodVo = DateRange.Create(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
        var totalHoursVo = Hours.Create(160);
        var hourlyRateVo = HourlyRate.Create(50);
        var providerIdVo = ProviderId.New();

        // Create domain entity for validation
        var timesheet = new Domain.TimesheetProcessing.Timesheet(
            providerId: providerIdVo,
            period: periodVo,
            totalHours: totalHoursVo,
            hourlyRate: hourlyRateVo
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

        // Create value objects
        var periodVo = DateRange.Create(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
        var totalHoursVo = Hours.Create(160);
        var hourlyRateVo = HourlyRate.Create(50);
        var providerIdVo = ProviderId.New();

        // In real implementation, this would load the timesheet from database
        var timesheet = new Domain.TimesheetProcessing.Timesheet(
            providerId: providerIdVo,
            period: periodVo,
            totalHours: totalHoursVo,
            hourlyRate: hourlyRateVo
        );

        // Calculate payroll with tax rate (e.g., 25%)
        const decimal taxRate = 0.25m;
        timesheet.CalculatePayroll(taxRate);

        await Task.Delay(100);

        Console.WriteLine($"[PayrollCalculation] Gross: {timesheet.GrossPay}, Tax: {timesheet.TaxAmount}, Net: {timesheet.NetPay}");

        return new PayrollCalculationResult(
            GrossPay: timesheet.GrossPay.Amount,
            TaxAmount: timesheet.TaxAmount.Amount,
            NetPay: timesheet.NetPay.Amount
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

        // Create value objects
        var periodVo = DateRange.Create(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
        var totalHoursVo = Hours.Create(160);
        var hourlyRateVo = HourlyRate.Create(50);
        var providerIdVo = ProviderId.New();

        // In real implementation, this would load the timesheet from database
        var timesheet = new Domain.TimesheetProcessing.Timesheet(
            providerId: providerIdVo,
            period: periodVo,
            totalHours: totalHoursVo,
            hourlyRate: hourlyRateVo
        );

        // Calculate facility bill (typically higher than provider pay rate)
        var facilityBillAmount = timesheet.TotalHours.Value * facilityBillRate;

        // Simulate external API call to ERP system to send invoice
        await Task.Delay(1000);

        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

        Console.WriteLine($"[InvoiceGeneration] Generated invoice {invoiceNumber} for timesheet {timesheetId}");
        Console.WriteLine($"[InvoiceGeneration] Facility bill amount: {facilityBillAmount:C} (Rate: {facilityBillRate:C}/hr)");

        return invoiceNumber;
    }
}
