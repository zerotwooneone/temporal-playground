using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.WorkflowOrchestration.ValueObjects;

public sealed record TemporalSignalName
{
    public string Value { get; }

    private TemporalSignalName(string value) => Value = value;

    public static Result<TemporalSignalName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<TemporalSignalName>.Failure("Temporal signal name cannot be null or empty");

        if (value.Length > 100)
            return Result<TemporalSignalName>.Failure("Temporal signal name cannot exceed 100 characters");

        return Result<TemporalSignalName>.Success(new TemporalSignalName(value.Trim()));
    }

    public override string ToString() => Value;
}
