using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.ProviderCredentialing;

public sealed record ProviderProfileId
{
    public const string Abbreviation = "PRP";
    public Guid Value { get; }

    private ProviderProfileId(Guid value) => Value = value;

    public static Result<ProviderProfileId> Create(string value)
    {
        if (string.IsNullOrEmpty(value))
            return Result<ProviderProfileId>.Failure("ProviderProfile ID cannot be null or empty");

        var expectedPrefix = $"{Abbreviation}Id";
        if (!value.StartsWith(expectedPrefix))
            return Result<ProviderProfileId>.Failure($"ProviderProfile ID must start with '{expectedPrefix}'");

        var guidString = value.Substring(expectedPrefix.Length);
        if (!Guid.TryParse(guidString, out var guid))
            return Result<ProviderProfileId>.Failure("Invalid GUID format in ProviderProfile ID");

        return Result<ProviderProfileId>.Success(new ProviderProfileId(guid));
    }

    public static ProviderProfileId New() => new ProviderProfileId(Guid.CreateVersion7());

    public override string ToString() => $"{Abbreviation}Id{Value}";
}
