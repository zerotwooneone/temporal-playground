namespace TemporalDDD.Domain.ProviderCredentialing.ValueObjects;

public sealed record ComplianceNotes
{
    public string Value { get; }

    private ComplianceNotes(string value) => Value = value;

    public static ComplianceNotes Create(string? value)
    {
        if (value != null && value.Length > 2000)
            throw new ArgumentException("Compliance notes cannot exceed 2000 characters", nameof(value));
        return new ComplianceNotes(value ?? string.Empty);
    }

    public static implicit operator string(ComplianceNotes notes) => notes.Value;
    public override string ToString() => Value;
}
