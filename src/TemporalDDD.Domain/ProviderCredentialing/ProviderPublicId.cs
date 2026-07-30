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

    public static ProviderPublicId FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ProviderPublicId cannot be null or whitespace", nameof(value));

        var parts = value.Split('_');
        if (parts.Length != 2)
            throw new ArgumentException("ProviderPublicId must be in format 'PREFIX_Guid'", nameof(value));

        if (parts[0] != Prefix)
            throw new ArgumentException($"ProviderPublicId must have prefix '{Prefix}'", nameof(value));

        var guidValue = Guid.Parse(parts[1]);
        if (guidValue == Guid.Empty)
            throw new ArgumentException("ProviderPublicId cannot be empty", nameof(value));

        return new ProviderPublicId(guidValue);
    }

    public static ProviderPublicId New() => new(Guid.NewGuid());

    public static implicit operator Guid(ProviderPublicId id) => id.Value;

    public override string ToString() => $"{Prefix}_{Value:N}";
}
