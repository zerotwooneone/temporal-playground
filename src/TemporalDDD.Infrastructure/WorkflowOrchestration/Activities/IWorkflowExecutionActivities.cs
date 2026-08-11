using System.Text.Json;
using Temporalio.Activities;
using TemporalDDD.Domain.WorkflowOrchestration.ValueObjects;

namespace TemporalDDD.Infrastructure.WorkflowOrchestration.Activities;

public interface IWorkflowExecutionActivities
{
    [Activity]
    Task<JsonDocument> ExecuteApiCallAsync(ExecuteApiInput input);

    [Activity]
    Task SendNotificationAsync(SendNotificationInput input);
}

public record ExecuteApiInput(
    string NodeId,
    string? EndpointUrl,
    string? AuthToken,
    ContractMapping? Mapping);

public record SendNotificationInput(
    string NodeId,
    string? MessageTemplate);
