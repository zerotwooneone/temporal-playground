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
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id.ToString(), cancellationToken);
        
        if (dbo == null) return null;
        
        return MapToDomain(dbo);
    }

    public async Task SaveAsync(Timesheet aggregate, CancellationToken cancellationToken = default)
    {
        var dbo = MapToDbo(aggregate);
        var idString = aggregate.Id.ToString();
        var existing = await _dbContext.Timesheets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == idString, cancellationToken);

        if (existing == null)
        {
            _dbContext.Timesheets.Add(dbo);
        }
        else
        {
            _dbContext.Timesheets.Update(dbo);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private Timesheet MapToDomain(TimesheetDbo dbo)
    {
        var providerId = ProviderId.Create(dbo.ProviderId).Value ?? throw new InvalidOperationException($"Invalid provider ID in database: {dbo.ProviderId}");
        var periodStart = DateTimeOffset.FromUnixTimeMilliseconds(dbo.PeriodStartUtc);
        var periodEnd = DateTimeOffset.FromUnixTimeMilliseconds(dbo.PeriodEndUtc);
        var period = DateRange.Create(periodStart, periodEnd).Value ?? throw new InvalidOperationException($"Invalid date range in database: {periodStart} to {periodEnd}");
        var totalHours = Hours.Create(dbo.TotalHours).Value ?? throw new InvalidOperationException($"Invalid total hours in database: {dbo.TotalHours}");
        var hourlyRate = HourlyRate.Create(dbo.HourlyRate).Value ?? throw new InvalidOperationException($"Invalid hourly rate in database: {dbo.HourlyRate}");
        var grossPay = Money.Create(decimal.Parse(dbo.GrossPayAmount), dbo.GrossPayCurrency).Value ?? throw new InvalidOperationException($"Invalid gross pay in database: {dbo.GrossPayAmount} {dbo.GrossPayCurrency}");
        var taxAmount = Money.Create(decimal.Parse(dbo.TaxAmount), dbo.TaxCurrency).Value ?? throw new InvalidOperationException($"Invalid tax amount in database: {dbo.TaxAmount} {dbo.TaxCurrency}");
        var netPay = Money.Create(decimal.Parse(dbo.NetPayAmount), dbo.NetPayCurrency).Value ?? throw new InvalidOperationException($"Invalid net pay in database: {dbo.NetPayAmount} {dbo.NetPayCurrency}");
        var status = TimesheetStatus.FromValue(dbo.Status);
        var submittedAt = DateTimeOffset.FromUnixTimeMilliseconds(dbo.SubmittedAt);
        var processedAt = dbo.ProcessedAt.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(dbo.ProcessedAt.Value) : (DateTimeOffset?)null;

        PaymentReference? paymentReference = null;
        if (!string.IsNullOrEmpty(dbo.PaymentReference))
        {
            paymentReference = PaymentReference.Create(dbo.PaymentReference).Value ?? throw new InvalidOperationException($"Invalid payment reference in database: {dbo.PaymentReference}");
        }

        TimesheetPublicId? publicId = null;
        if (!string.IsNullOrEmpty(dbo.PublicId))
        {
            publicId = TimesheetPublicId.FromString(dbo.PublicId);
        }

        var id = TimesheetId.Create(dbo.Id).Value ?? throw new InvalidOperationException($"Invalid timesheet ID in database: {dbo.Id}");

        // Use internal constructor for rehydration (infrastructure concern)
        return new Timesheet(
            id: id,
            publicId: publicId,
            providerId: providerId,
            period: period,
            totalHours: totalHours,
            hourlyRate: hourlyRate,
            grossPay: grossPay,
            taxAmount: taxAmount,
            netPay: netPay,
            status: status,
            submittedAt: submittedAt,
            processedAt: processedAt,
            paymentReference: paymentReference,
            rejectionReason: dbo.RejectionReason
        );
    }

    private TimesheetDbo MapToDbo(Timesheet timesheet)
    {
        return new TimesheetDbo
        {
            Id = timesheet.Id.ToString(),
            PublicId = timesheet.PublicId?.ToString(),
            ProviderId = timesheet.ProviderId.ToString(),
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
