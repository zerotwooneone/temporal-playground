using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.ProviderCredentialing;

public sealed record CredentialEvaluationPublicId
{
    private const string Prefix = "CRE";
    public Guid Value { get; }

    private CredentialEvaluationPublicId(Guid value)
    {
        Value = value;
    }

    public static Result<CredentialEvaluationPublicId> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<CredentialEvaluationPublicId>.Failure("CredentialEvaluationPublicId cannot be null or whitespace");

        var parts = value.Split('_');
        if (parts.Length != 2)
            return Result<CredentialEvaluationPublicId>.Failure("CredentialEvaluationPublicId must be in format 'PREFIX_Guid'");

        if (parts[0] != Prefix)
            return Result<CredentialEvaluationPublicId>.Failure($"CredentialEvaluationPublicId must have prefix '{Prefix}'");

        if (!Guid.TryParse(parts[1], out var guidValue))
            return Result<CredentialEvaluationPublicId>.Failure("Invalid GUID format in CredentialEvaluationPublicId");

        if (guidValue == Guid.Empty)
            return Result<CredentialEvaluationPublicId>.Failure("CredentialEvaluationPublicId cannot be empty");

        return Result<CredentialEvaluationPublicId>.Success(new CredentialEvaluationPublicId(guidValue));
    }

    public static CredentialEvaluationPublicId New() => new(Guid.NewGuid());

    public static implicit operator Guid(CredentialEvaluationPublicId id) => id.Value;

    public override string ToString() => $"{Prefix}_{Value:N}";
}
