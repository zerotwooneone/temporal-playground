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

    public static TimesheetPublicId FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("TimesheetPublicId cannot be null or whitespace", nameof(value));

        var parts = value.Split('_');
        if (parts.Length != 2)
            throw new ArgumentException("TimesheetPublicId must be in format 'PREFIX_Guid'", nameof(value));

        if (parts[0] != Prefix)
            throw new ArgumentException($"TimesheetPublicId must have prefix '{Prefix}'", nameof(value));

        var guidValue = Guid.Parse(parts[1]);
        if (guidValue == Guid.Empty)
            throw new ArgumentException("TimesheetPublicId cannot be empty", nameof(value));

        return new TimesheetPublicId(guidValue);
    }

    public static TimesheetPublicId New() => new(Guid.NewGuid());

    public static implicit operator Guid(TimesheetPublicId id) => id.Value;

    public override string ToString() => $"{Prefix}_{Value:N}";
}
