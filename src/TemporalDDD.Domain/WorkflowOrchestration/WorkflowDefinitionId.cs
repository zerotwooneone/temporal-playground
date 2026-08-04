using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.WorkflowOrchestration;

public sealed record WorkflowDefinitionId
{
    private const string Abbreviation = "WFL";
    public Guid Value { get; }

    private WorkflowDefinitionId(Guid value) => Value = value;

    public static Result<WorkflowDefinitionId> Create(string value)
    {
        if (string.IsNullOrEmpty(value))
            return Result<WorkflowDefinitionId>.Failure("WorkflowDefinition ID cannot be null or empty");

        var expectedPrefix = $"{Abbreviation}Id";
        if (!value.StartsWith(expectedPrefix))
            return Result<WorkflowDefinitionId>.Failure($"WorkflowDefinition ID must start with '{expectedPrefix}'");

        var guidString = value.Substring(expectedPrefix.Length);
        if (!Guid.TryParse(guidString, out var guid))
            return Result<WorkflowDefinitionId>.Failure("Invalid GUID format in WorkflowDefinition ID");

        return Result<WorkflowDefinitionId>.Success(new WorkflowDefinitionId(guid));
    }

    public static WorkflowDefinitionId New() => new WorkflowDefinitionId(Guid.CreateVersion7());

    public override string ToString() => $"{Abbreviation}Id{Value}";
}
