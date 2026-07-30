using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.PlacementMatching;

public sealed record FacilityId
{
    private const string Abbreviation = "FAC";
    public Guid Value { get; }

    private FacilityId(Guid value)
    {
        Value = value;
    }

    public static Result<FacilityId> Create(string value)
    {
        if (string.IsNullOrEmpty(value))
            return Result<FacilityId>.Failure("Facility ID cannot be null or empty");

        var expectedPrefix = $"{Abbreviation}Id";
        if (!value.StartsWith(expectedPrefix))
            return Result<FacilityId>.Failure($"Facility ID must start with '{expectedPrefix}'");

        var guidString = value.Substring(expectedPrefix.Length);
        if (!Guid.TryParse(guidString, out var guid))
            return Result<FacilityId>.Failure("Invalid GUID format in Facility ID");

        return Result<FacilityId>.Success(new FacilityId(guid));
    }

    public static FacilityId New() => new FacilityId(Guid.CreateVersion7());

    public override string ToString() => $"{Abbreviation}Id{Value}";
}
