namespace TemporalDDD.Domain.TimesheetProcessing;

public class Timesheet
{
    public Guid Id { get; private set; }
    public Guid ProviderId { get; private set; }
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    public decimal TotalHours { get; private set; }
    public decimal HourlyRate { get; private set; }
    public decimal GrossPay { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal NetPay { get; private set; }
    public TimesheetStatus Status { get; private set; }
    public DateTime SubmittedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? PaymentReference { get; private set; }

    private Timesheet() { }

    public Timesheet(Guid providerId, DateTime periodStart, DateTime periodEnd, decimal totalHours, decimal hourlyRate)
    {
        Id = Guid.NewGuid();
        ProviderId = providerId;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        TotalHours = totalHours;
        HourlyRate = hourlyRate;
        GrossPay = totalHours * hourlyRate;
        Status = TimesheetStatus.Submitted;
        SubmittedAt = DateTime.UtcNow;
    }

    public void Validate()
    {
        if (TotalHours <= 0)
            throw new InvalidOperationException("Total hours must be greater than zero");

        if (TotalHours > 160) // 40 hours/week * 4 weeks
            throw new InvalidOperationException("Total hours cannot exceed 160 for a 4-week period");

        if (HourlyRate <= 0)
            throw new InvalidOperationException("Hourly rate must be greater than zero");

        if (PeriodEnd <= PeriodStart)
            throw new InvalidOperationException("Period end must be after period start");
    }

    public void CalculatePayroll(decimal taxRate)
    {
        TaxAmount = GrossPay * taxRate;
        NetPay = GrossPay - TaxAmount;
    }

    public void MarkAsProcessed(string paymentReference)
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
