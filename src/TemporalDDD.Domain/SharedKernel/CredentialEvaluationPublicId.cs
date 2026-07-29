namespace TemporalDDD.Domain.SharedKernel;

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

    public static CredentialEvaluationPublicId New() => new(Guid.NewGuid());

    public static implicit operator Guid(CredentialEvaluationPublicId id) => id.Value;

    public override string ToString() => $"{Prefix}_{Value:N}";
}
