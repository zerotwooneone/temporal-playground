namespace TemporalDDD.Domain.ProviderCredentialing;

public sealed record CredentialEvaluationPublicId
{
    private const string Prefix = "CRE";
    public Guid Value { get; }

    private CredentialEvaluationPublicId(Guid value)
    {
        Value = value;
    }

    public static CredentialEvaluationPublicId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CredentialEvaluationPublicId cannot be empty", nameof(value));

        return new CredentialEvaluationPublicId(value);
    }

    public static CredentialEvaluationPublicId FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("CredentialEvaluationPublicId cannot be null or whitespace", nameof(value));

        var parts = value.Split('_');
        if (parts.Length != 2)
            throw new ArgumentException("CredentialEvaluationPublicId must be in format 'PREFIX_Guid'", nameof(value));

        if (parts[0] != Prefix)
            throw new ArgumentException($"CredentialEvaluationPublicId must have prefix '{Prefix}'", nameof(value));

        var guidValue = Guid.Parse(parts[1]);
        if (guidValue == Guid.Empty)
            throw new ArgumentException("CredentialEvaluationPublicId cannot be empty", nameof(value));

        return new CredentialEvaluationPublicId(guidValue);
    }

    public static CredentialEvaluationPublicId New() => new(Guid.NewGuid());

    public static implicit operator Guid(CredentialEvaluationPublicId id) => id.Value;

    public override string ToString() => $"{Prefix}_{Value:N}";
}
