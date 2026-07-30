using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.PlacementMatching;

public sealed record PositionId
{
    private const string Abbreviation = "POS";
    public Guid Value { get; }

    private PositionId(Guid value)
    {
        Value = value;
    }

    public static Result<PositionId> Create(string value)
    {
        if (string.IsNullOrEmpty(value))
            return Result<PositionId>.Failure("Position ID cannot be null or empty");

        var expectedPrefix = $"{Abbreviation}Id";
        if (!value.StartsWith(expectedPrefix))
            return Result<PositionId>.Failure($"Position ID must start with '{expectedPrefix}'");

        var guidString = value.Substring(expectedPrefix.Length);
        if (!Guid.TryParse(guidString, out var guid))
            return Result<PositionId>.Failure("Invalid GUID format in Position ID");

        return Result<PositionId>.Success(new PositionId(guid));
    }

    public static PositionId New() => new PositionId(Guid.CreateVersion7());

    public override string ToString() => $"{Abbreviation}Id{Value}";
}
