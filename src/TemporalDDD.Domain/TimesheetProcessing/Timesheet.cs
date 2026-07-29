using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.TimesheetProcessing.ValueObjects;

namespace TemporalDDD.Domain.TimesheetProcessing;

public class Timesheet
{
    public Guid Id { get; private set; }
    public Guid ProviderId { get; private set; }
    public DateRange Period { get; private set; }
    public Hours TotalHours { get; private set; }
    public HourlyRate HourlyRate { get; private set; }
    public Money GrossPay { get; private set; }
    public Money TaxAmount { get; private set; }
    public Money NetPay { get; private set; }
    public TimesheetStatus Status { get; private set; }
    public DateTime SubmittedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public PaymentReference? PaymentReference { get; private set; }

    private Timesheet() { }

    public Timesheet(Guid providerId, DateRange period, Hours totalHours, HourlyRate hourlyRate)
    {
        Id = Guid.NewGuid();
        ProviderId = providerId;
        Period = period;
        TotalHours = totalHours;
        HourlyRate = hourlyRate;
        GrossPay = hourlyRate.CalculatePay(totalHours);
        Status = TimesheetStatus.Submitted;
        SubmittedAt = DateTime.UtcNow;
    }

    public void Validate()
    {
        // Value objects already enforce their own validation
        // Additional business validation if needed
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
        if (Status != TimesheetStatus.Submitted)
            throw new InvalidOperationException($"Cannot process timesheet in status: {Status}");

        Status = TimesheetStatus.Processed;
        ProcessedAt = DateTime.UtcNow;
        PaymentReference = paymentReference;
    }
}

public enum TimesheetStatus
{
    Submitted,
    Validated,
    Processed,
    Failed
}
