using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.ProviderCredentialing;

public sealed record ProviderPublicId
{
    private const string Prefix = "PRV";
    public Guid Value { get; }

    private ProviderPublicId(Guid value)
    {
        Value = value;
    }

    public static Result<ProviderPublicId> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<ProviderPublicId>.Failure("ProviderPublicId cannot be null or whitespace");

        var parts = value.Split('_');
        if (parts.Length != 2)
            return Result<ProviderPublicId>.Failure("ProviderPublicId must be in format 'PREFIX_Guid'");

        if (parts[0] != Prefix)
            return Result<ProviderPublicId>.Failure($"ProviderPublicId must have prefix '{Prefix}'");

        if (!Guid.TryParse(parts[1], out var guidValue))
            return Result<ProviderPublicId>.Failure("Invalid GUID format in ProviderPublicId");

        if (guidValue == Guid.Empty)
            return Result<ProviderPublicId>.Failure("ProviderPublicId cannot be empty");

        return Result<ProviderPublicId>.Success(new ProviderPublicId(guidValue));
    }

    public static ProviderPublicId New() => new(Guid.NewGuid());

    public static implicit operator Guid(ProviderPublicId id) => id.Value;

    public override string ToString() => $"{Prefix}_{Value:N}";
}
