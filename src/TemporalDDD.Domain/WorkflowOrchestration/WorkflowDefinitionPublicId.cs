using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.WorkflowOrchestration;

public sealed record WorkflowDefinitionPublicId
{
    private const string Prefix = "WFL";
    public Guid Value { get; }

    private WorkflowDefinitionPublicId(Guid value)
    {
        Value = value;
    }

    public static Result<WorkflowDefinitionPublicId> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<WorkflowDefinitionPublicId>.Failure("WorkflowDefinitionPublicId cannot be null or whitespace");

        var parts = value.Split('_');
        if (parts.Length != 2)
            return Result<WorkflowDefinitionPublicId>.Failure("WorkflowDefinitionPublicId must be in format 'PREFIX_Guid'");

        if (parts[0] != Prefix)
            return Result<WorkflowDefinitionPublicId>.Failure($"WorkflowDefinitionPublicId must have prefix '{Prefix}'");

        if (!Guid.TryParse(parts[1], out var guidValue))
            return Result<WorkflowDefinitionPublicId>.Failure("Invalid GUID format in WorkflowDefinitionPublicId");

        if (guidValue == Guid.Empty)
            return Result<WorkflowDefinitionPublicId>.Failure("WorkflowDefinitionPublicId cannot be empty");

        return Result<WorkflowDefinitionPublicId>.Success(new WorkflowDefinitionPublicId(guidValue));
    }

    public static WorkflowDefinitionPublicId New() => new(Guid.NewGuid());

    public static implicit operator Guid(WorkflowDefinitionPublicId id) => id.Value;

    public override string ToString() => $"{Prefix}_{Value:N}";
}
