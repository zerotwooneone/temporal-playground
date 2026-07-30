using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.TimesheetProcessing.ValueObjects;

namespace TemporalDDD.Domain.TimesheetProcessing;

public sealed class Timesheet
{
    public TimesheetId Id { get; private set; }
    public TimesheetPublicId? PublicId { get; private set; }
    public ProviderId ProviderId { get; private set; }
    public DateRange Period { get; private set; }
    public Hours TotalHours { get; private set; }
    public HourlyRate HourlyRate { get; private set; }
    public Money GrossPay { get; private set; }
    public Money TaxAmount { get; private set; }
    public Money NetPay { get; private set; }
    public TimesheetStatus Status { get; private set; }
    public DateTimeOffset SubmittedAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public PaymentReference? PaymentReference { get; private set; }
    public string? RejectionReason { get; private set; }

    private Timesheet() { }

    // Factory for creating new timesheet (ID will be set by database)
    public static Timesheet Create(ProviderId providerId, DateRange period, Hours totalHours, HourlyRate hourlyRate)
    {
        return new Timesheet
        {
            Id = TimesheetId.Create(0), // Temporary, will be set by DB
            PublicId = TimesheetPublicId.New(),
            ProviderId = providerId,
            Period = period,
            TotalHours = totalHours,
            HourlyRate = hourlyRate,
            GrossPay = hourlyRate.CalculatePay(totalHours),
            Status = TimesheetStatus.Submitted,
            SubmittedAt = DateTimeOffset.UtcNow
        };
    }

    

    public void Validate()
    {
        if (Status != TimesheetStatus.Submitted)
            throw new InvalidOperationException($"Cannot validate timesheet in status: {Status}");

        Status = TimesheetStatus.Validated;
    }

    public void Approve()
    {
        if (Status != TimesheetStatus.Validated)
            throw new InvalidOperationException($"Cannot approve timesheet in status: {Status}");

        Status = TimesheetStatus.Approved;
    }

    public void Reject(string reason)
    {
        if (Status != TimesheetStatus.Validated && Status != TimesheetStatus.Submitted)
            throw new InvalidOperationException($"Cannot reject timesheet in status: {Status}");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Rejection reason cannot be null or whitespace", nameof(reason));

        Status = TimesheetStatus.Rejected;
        RejectionReason = reason;
    }

    public void Submit()
    {
        if (Status != TimesheetStatus.Rejected)
            throw new InvalidOperationException($"Cannot resubmit timesheet in status: {Status}");

        Status = TimesheetStatus.Submitted;
        RejectionReason = null;
    }

    public Money CalculateGrossPay(decimal overtimeMultiplier = 1.5m)
    {
        // Standard pay for first 40 hours
        var standardHours = TotalHours.Value > 40.0m ? 40.0m : TotalHours.Value;
        var standardPay = Money.Create(standardHours * HourlyRate.Value, "USD");

        // Overtime pay for hours over 40
        if (TotalHours.Value > 40.0m)
        {
            var overtimeHours = TotalHours.Value - 40.0m;
            var overtimePay = Money.Create(overtimeHours * HourlyRate.Value * overtimeMultiplier, "USD");
            return standardPay + overtimePay;
        }

        return standardPay;
    }

    public void CalculatePayroll(decimal taxRate)
    {
        if (taxRate < 0 || taxRate > 1)
            throw new ArgumentException("Tax rate must be between 0 and 1", nameof(taxRate));

        var taxAmount = GrossPay.Amount * taxRate;
        TaxAmount = Money.Create(taxAmount, GrossPay.Currency);
        NetPay = GrossPay - TaxAmount;
    }

    public void MarkAsProcessed(PaymentReference paymentReference)
    {
        if (Status != TimesheetStatus.Approved)
            throw new InvalidOperationException($"Cannot process timesheet in status: {Status}");

        Status = TimesheetStatus.Processed;
        ProcessedAt = DateTimeOffset.UtcNow;
        PaymentReference = paymentReference;
    }
}
