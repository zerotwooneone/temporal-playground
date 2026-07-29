using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.TimesheetProcessing.ValueObjects;

namespace TemporalDDD.Domain.TimesheetProcessing;

public class Timesheet
{
    public uint Id { get; private set; }
    public TimesheetPublicId? PublicId { get; private set; }
    public ProviderId ProviderId { get; private set; }
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

    // Factory for creating new timesheet (ID will be set by database)
    public static Timesheet Create(ProviderId providerId, DateRange period, Hours totalHours, HourlyRate hourlyRate)
    {
        return new Timesheet
        {
            Id = 0, // Temporary, will be set by DB
            PublicId = TimesheetPublicId.New(),
            ProviderId = providerId,
            Period = period,
            TotalHours = totalHours,
            HourlyRate = hourlyRate,
            GrossPay = hourlyRate.CalculatePay(totalHours),
            Status = TimesheetStatus.Submitted,
            SubmittedAt = DateTime.UtcNow
        };
    }

    // Factory for rehydrating from database
    public static Timesheet FromDatabase(uint id, Guid? publicId, ProviderId providerId, DateRange period, Hours totalHours, HourlyRate hourlyRate, Money grossPay, Money taxAmount, Money netPay, TimesheetStatus status, DateTime submittedAt, DateTime? processedAt, PaymentReference? paymentReference)
    {
        return new Timesheet
        {
            Id = id,
            PublicId = publicId.HasValue ? TimesheetPublicId.Create(publicId.Value) : null,
            ProviderId = providerId,
            Period = period,
            TotalHours = totalHours,
            HourlyRate = hourlyRate,
            GrossPay = grossPay,
            TaxAmount = taxAmount,
            NetPay = netPay,
            Status = status,
            SubmittedAt = submittedAt,
            ProcessedAt = processedAt,
            PaymentReference = paymentReference
        };
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
