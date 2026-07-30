namespace TemporalDDD.Domain.ProviderCredentialing;

public sealed record ProviderPublicId
{
    private const string Prefix = "PRV";
    public Guid Value { get; }

    private ProviderPublicId(Guid value)
    {
        Value = value;
    }

    public static ProviderPublicId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ProviderPublicId cannot be empty", nameof(value));

        return new ProviderPublicId(value);
    }

    public static ProviderPublicId New() => new(Guid.NewGuid());

    public static implicit operator Guid(ProviderPublicId id) => id.Value;

    public override string ToString() => $"{Prefix}_{Value:N}";
}
