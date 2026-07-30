namespace TemporalDDD.Domain.TimesheetProcessing;

public sealed record TimesheetStatus
{
    public int Value { get; }
    public string Name { get; }

    private TimesheetStatus(int value, string name)
    {
        Value = value;
        Name = name;
    }

    public static readonly TimesheetStatus Submitted = new(0, "Submitted");
    public static readonly TimesheetStatus Validated = new(1, "Validated");
    public static readonly TimesheetStatus Approved = new(2, "Approved");
    public static readonly TimesheetStatus Rejected = new(3, "Rejected");
    public static readonly TimesheetStatus Processed = new(4, "Processed");
    public static readonly TimesheetStatus Failed = new(5, "Failed");

    private static readonly TimesheetStatus[] AllStatuses = { Submitted, Validated, Approved, Rejected, Processed, Failed };

    public static TimesheetStatus FromValue(int value)
    {
        return AllStatuses.FirstOrDefault(s => s.Value == value) 
            ?? throw new ArgumentException($"Invalid TimesheetStatus value: {value}", nameof(value));
    }

    public static implicit operator int(TimesheetStatus status) => status.Value;

    public override string ToString() => Name;
}
