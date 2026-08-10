using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.WorkflowOrchestration;

public sealed record BusinessReferenceId
{
    private const string Abbreviation = "BRF";
    public Guid Value { get; }

    private BusinessReferenceId(Guid value) => Value = value;

    public static Result<BusinessReferenceId> Create(string value)
    {
        if (string.IsNullOrEmpty(value))
            return Result<BusinessReferenceId>.Failure("Business Reference ID cannot be null or empty");

        var expectedPrefix = $"{Abbreviation}Id";
        if (!value.StartsWith(expectedPrefix))
            return Result<BusinessReferenceId>.Failure($"Business Reference ID must start with '{expectedPrefix}'");

        var guidString = value.Substring(expectedPrefix.Length);
        if (!Guid.TryParse(guidString, out var guid))
            return Result<BusinessReferenceId>.Failure("Invalid GUID format in Business Reference ID");

        return Result<BusinessReferenceId>.Success(new BusinessReferenceId(guid));
    }

    public static BusinessReferenceId New() => new BusinessReferenceId(Guid.NewGuid());

    public override string ToString() => $"{Abbreviation}Id{Value}";
}
