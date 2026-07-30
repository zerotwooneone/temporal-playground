using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.ProviderCredentialing.ValueObjects;

public sealed record ComplianceNotes
{
    public string Value { get; }

    private ComplianceNotes(string value) => Value = value;

    public static Result<ComplianceNotes> Create(string? value)
    {
        if (value != null && value.Length > 2000)
            return Result<ComplianceNotes>.Failure("Compliance notes cannot exceed 2000 characters");
        return Result<ComplianceNotes>.Success(new ComplianceNotes(value ?? string.Empty));
    }

    public static implicit operator string(ComplianceNotes notes) => notes.Value;
    public override string ToString() => Value;
}
