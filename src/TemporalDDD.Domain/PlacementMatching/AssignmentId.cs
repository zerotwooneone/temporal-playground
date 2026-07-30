using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.PlacementMatching;

public sealed record AssignmentId
{
    private const string Abbreviation = "ASM";
    public Guid Value { get; }

    private AssignmentId(Guid value)
    {
        Value = value;
    }

    public static Result<AssignmentId> Create(string value)
    {
        if (string.IsNullOrEmpty(value))
            return Result<AssignmentId>.Failure("Assignment ID cannot be null or empty");

        var expectedPrefix = $"{Abbreviation}Id";
        if (!value.StartsWith(expectedPrefix))
            return Result<AssignmentId>.Failure($"Assignment ID must start with '{expectedPrefix}'");

        var guidString = value.Substring(expectedPrefix.Length);
        if (!Guid.TryParse(guidString, out var guid))
            return Result<AssignmentId>.Failure("Invalid GUID format in Assignment ID");

        return Result<AssignmentId>.Success(new AssignmentId(guid));
    }

    public static AssignmentId New() => new AssignmentId(Guid.CreateVersion7());

    public override string ToString() => $"{Abbreviation}Id{Value}";
}
