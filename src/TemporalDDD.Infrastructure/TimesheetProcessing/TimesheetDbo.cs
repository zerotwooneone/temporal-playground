namespace TemporalDDD.Infrastructure.TimesheetProcessing;

public class TimesheetDbo
{
    public uint Id { get; set; }
    public string? PublicId { get; set; }
    public uint ProviderId { get; set; }
    public long PeriodStartUtc { get; set; } // Unix milliseconds
    public long PeriodEndUtc { get; set; } // Unix milliseconds
    public decimal TotalHours { get; set; }
    public decimal HourlyRate { get; set; }
    public string GrossPayAmount { get; set; }
    public string GrossPayCurrency { get; set; }
    public string TaxAmount { get; set; }
    public string TaxCurrency { get; set; }
    public string NetPayAmount { get; set; }
    public string NetPayCurrency { get; set; }
    public int Status { get; set; }
    public long SubmittedAt { get; set; } // Unix milliseconds
    public long? ProcessedAt { get; set; } // Unix milliseconds
    public string? PaymentReference { get; set; }
    public string? RejectionReason { get; set; }
}
