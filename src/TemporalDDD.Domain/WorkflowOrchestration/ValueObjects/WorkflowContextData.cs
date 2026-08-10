using System.Text.Json;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.WorkflowOrchestration.ValueObjects;

public sealed record WorkflowContextData
{
    public string Json { get; }

    private WorkflowContextData(string json) => Json = json;

    public static Result<WorkflowContextData> Create(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Result<WorkflowContextData>.Success(new WorkflowContextData("{}"));

        try
        {
            JsonDocument.Parse(json);
            return Result<WorkflowContextData>.Success(new WorkflowContextData(json));
        }
        catch (JsonException)
        {
            return Result<WorkflowContextData>.Failure("Workflow context data must be valid JSON");
        }
    }

    public static WorkflowContextData Empty() => new WorkflowContextData("{}");

    public override string ToString() => Json;
}
