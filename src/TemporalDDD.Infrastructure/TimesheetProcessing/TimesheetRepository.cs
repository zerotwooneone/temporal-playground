using Microsoft.EntityFrameworkCore;
using TemporalDDD.Domain.TimesheetProcessing;
using TemporalDDD.Domain.TimesheetProcessing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.TimesheetProcessing;

public class TimesheetRepository : ITimesheetRepository
{
    private readonly ApplicationDbContext _dbContext;

    public TimesheetRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Timesheet?> GetByIdAsync(TimesheetId id, CancellationToken cancellationToken = default)
    {
        var dbo = await _dbContext.Timesheets
            .FirstOrDefaultAsync(t => t.Id == id.Value, cancellationToken);
        
        if (dbo == null) return null;
        
        return MapToDomain(dbo);
    }

    public async Task SaveAsync(Timesheet aggregate, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Timesheets
            .FirstOrDefaultAsync(t => t.Id == aggregate.Id.Value, cancellationToken);

        var dbo = MapToDbo(aggregate);

        if (existing == null)
        {
            _dbContext.Timesheets.Add(dbo);
        }
        else
        {
            _dbContext.Entry(existing).State = EntityState.Detached;
            _dbContext.Timesheets.Attach(dbo);
            _dbContext.Entry(dbo).State = EntityState.Modified;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private Timesheet MapToDomain(TimesheetDbo dbo)
    {
        var providerId = ProviderId.Create(dbo.ProviderId).Value;
        var periodStart = DateTimeOffset.FromUnixTimeMilliseconds(dbo.PeriodStartUtc);
        var periodEnd = DateTimeOffset.FromUnixTimeMilliseconds(dbo.PeriodEndUtc);
        var period = DateRange.Create(periodStart, periodEnd);
        var totalHours = Hours.Create(dbo.TotalHours).Value;
        var hourlyRate = HourlyRate.Create(dbo.HourlyRate).Value;
        var grossPay = Money.Create(decimal.Parse(dbo.GrossPayAmount), dbo.GrossPayCurrency);
        var taxAmount = Money.Create(decimal.Parse(dbo.TaxAmount), dbo.TaxCurrency);
        var netPay = Money.Create(decimal.Parse(dbo.NetPayAmount), dbo.NetPayCurrency);
        var status = TimesheetStatus.FromValue(dbo.Status);
        var submittedAt = DateTimeOffset.FromUnixTimeMilliseconds(dbo.SubmittedAt);
        var processedAt = dbo.ProcessedAt.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(dbo.ProcessedAt.Value) : (DateTimeOffset?)null;

        PaymentReference? paymentReference = null;
        if (!string.IsNullOrEmpty(dbo.PaymentReference))
        {
            paymentReference = PaymentReference.Create(dbo.PaymentReference);
        }

        TimesheetPublicId? publicId = null;
        if (!string.IsNullOrEmpty(dbo.PublicId))
        {
            publicId = TimesheetPublicId.FromString(dbo.PublicId);
        }

        // Use reflection to call private constructor for rehydration
        var timesheet = (Timesheet)Activator.CreateInstance(
            typeof(Timesheet),
            nonPublic: true)!;
        
        // Set properties via reflection (infrastructure concern)
        typeof(Timesheet).GetProperty(nameof(Timesheet.Id))?.SetValue(timesheet, TimesheetId.Create(dbo.Id).Value);
        typeof(Timesheet).GetProperty(nameof(Timesheet.PublicId))?.SetValue(timesheet, publicId);
        typeof(Timesheet).GetProperty(nameof(Timesheet.ProviderId))?.SetValue(timesheet, providerId);
        typeof(Timesheet).GetProperty(nameof(Timesheet.Period))?.SetValue(timesheet, period);
        typeof(Timesheet).GetProperty(nameof(Timesheet.TotalHours))?.SetValue(timesheet, totalHours);
        typeof(Timesheet).GetProperty(nameof(Timesheet.HourlyRate))?.SetValue(timesheet, hourlyRate);
        typeof(Timesheet).GetProperty(nameof(Timesheet.GrossPay))?.SetValue(timesheet, grossPay);
        typeof(Timesheet).GetProperty(nameof(Timesheet.TaxAmount))?.SetValue(timesheet, taxAmount);
        typeof(Timesheet).GetProperty(nameof(Timesheet.NetPay))?.SetValue(timesheet, netPay);
        typeof(Timesheet).GetProperty(nameof(Timesheet.Status))?.SetValue(timesheet, status);
        typeof(Timesheet).GetProperty(nameof(Timesheet.SubmittedAt))?.SetValue(timesheet, submittedAt);
        typeof(Timesheet).GetProperty(nameof(Timesheet.ProcessedAt))?.SetValue(timesheet, processedAt);
        typeof(Timesheet).GetProperty(nameof(Timesheet.PaymentReference))?.SetValue(timesheet, paymentReference);
        typeof(Timesheet).GetProperty(nameof(Timesheet.RejectionReason))?.SetValue(timesheet, dbo.RejectionReason);

        return timesheet;
    }

    private TimesheetDbo MapToDbo(Timesheet timesheet)
    {
        return new TimesheetDbo
        {
            Id = timesheet.Id.Value,
            PublicId = timesheet.PublicId?.ToString(),
            ProviderId = timesheet.ProviderId.Value,
            PeriodStartUtc = timesheet.Period.Start.ToUnixTimeMilliseconds(),
            PeriodEndUtc = timesheet.Period.End.ToUnixTimeMilliseconds(),
            TotalHours = timesheet.TotalHours.Value,
            HourlyRate = timesheet.HourlyRate.Value,
            GrossPayAmount = timesheet.GrossPay.Amount.ToString(),
            GrossPayCurrency = timesheet.GrossPay.Currency,
            TaxAmount = timesheet.TaxAmount.Amount.ToString(),
            TaxCurrency = timesheet.TaxAmount.Currency,
            NetPayAmount = timesheet.NetPay.Amount.ToString(),
            NetPayCurrency = timesheet.NetPay.Currency,
            Status = timesheet.Status.Value,
            SubmittedAt = timesheet.SubmittedAt.ToUnixTimeMilliseconds(),
            ProcessedAt = timesheet.ProcessedAt?.ToUnixTimeMilliseconds(),
            PaymentReference = timesheet.PaymentReference?.Value,
            RejectionReason = timesheet.RejectionReason
        };
    }
}
