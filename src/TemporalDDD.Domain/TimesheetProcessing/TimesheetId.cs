namespace TemporalDDD.Domain.TimesheetProcessing;

public sealed record TimesheetId
{
    public uint Value { get; }

    private TimesheetId(uint value) => Value = value;

    public static TimesheetId Create(uint value)
    {
        if (value == 0)
            throw new ArgumentException("TimesheetId cannot be zero", nameof(value));
        return new TimesheetId(value);
    }

    public static TimesheetId FromDatabase(uint value) => new(value);
    public static implicit operator uint(TimesheetId id) => id.Value;
    public override string ToString() => Value.ToString();
}
