namespace TemporalDDD.Domain.TimesheetProcessing;

public sealed record TimesheetPublicId
{
    private const string Prefix = "TSH";
    public Guid Value { get; }

    private TimesheetPublicId(Guid value)
    {
        Value = value;
    }

    public static TimesheetPublicId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TimesheetPublicId cannot be empty", nameof(value));

        return new TimesheetPublicId(value);
    }

    public static TimesheetPublicId New() => new(Guid.NewGuid());

    public static implicit operator Guid(TimesheetPublicId id) => id.Value;

    public override string ToString() => $"{Prefix}_{Value:N}";
}
