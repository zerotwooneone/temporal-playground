using TemporalDDD.Application.Messaging;

namespace TemporalDDD.Application.WorkflowOrchestration;

public record CreateWorkflowDraftInput(
    string CreatorId,
    string Name);

public record UpdateWorkflowNodesInput(
    string WorkflowId,
    IReadOnlyList<WorkflowNodeDto> Nodes,
    IReadOnlyList<WorkflowTransitionDto> Transitions);

public record WorkflowTransitionDto(
    string SourceNodeId,
    string TargetNodeId);

public record WorkflowNodeDto(
    string Id,
    int NodeType,
    string Name,
    string? BusinessNotes,
    bool IsConfigured,
    // Api Node properties
    string? EndpointUrl,
    string? AuthToken,
    int? RetryPolicyMaxAttempts,
    int? RetryPolicyBackoffCoefficient,
    bool? ContractMappingConvertXmlToJson,
    string? ContractMappingQueryParameters,
    string? ContractMappingRequestMapping,
    string? ContractMappingResponseMapping,
    // Notification Node properties
    string? MessageTemplate
);

public record SaveWorkflowResult(
    string WorkflowId,
    IReadOnlyList<IApplicationEvent> Events);

public record PublishEventsInput(
    IReadOnlyList<IApplicationEvent> Events);
