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
    public static readonly TimesheetStatus Processed = new(2, "Processed");
    public static readonly TimesheetStatus Failed = new(3, "Failed");

    private static readonly TimesheetStatus[] AllStatuses = { Submitted, Validated, Processed, Failed };

    public static TimesheetStatus FromValue(int value)
    {
        return AllStatuses.FirstOrDefault(s => s.Value == value) 
            ?? throw new ArgumentException($"Invalid TimesheetStatus value: {value}", nameof(value));
    }

    public static implicit operator int(TimesheetStatus status) => status.Value;

    public override string ToString() => Name;
}
