namespace TemporalDDD.Domain.SharedKernel;

public sealed record ProviderId
{
    private const string Abbreviation = "PRV";
    public Guid Value { get; }

    private ProviderId(Guid value)
    {
        Value = value;
    }

    public static Result<ProviderId> Create(string value)
    {
        if (string.IsNullOrEmpty(value))
            return Result<ProviderId>.Failure("Provider ID cannot be null or empty");

        var expectedPrefix = $"{Abbreviation}Id";
        if (!value.StartsWith(expectedPrefix))
            return Result<ProviderId>.Failure($"Provider ID must start with '{expectedPrefix}'");

        var guidString = value.Substring(expectedPrefix.Length);
        if (!Guid.TryParse(guidString, out var guid))
            return Result<ProviderId>.Failure("Invalid GUID format in Provider ID");

        return Result<ProviderId>.Success(new ProviderId(guid));
    }

    public static ProviderId New() => new ProviderId(Guid.CreateVersion7());

    public override string ToString() => $"{Abbreviation}Id{Value}";
}
