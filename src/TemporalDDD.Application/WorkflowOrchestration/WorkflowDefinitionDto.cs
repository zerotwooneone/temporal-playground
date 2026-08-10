namespace TemporalDDD.Application.WorkflowOrchestration;

public record WorkflowDefinitionDto(
    string Id,
    string PublicId,
    string Name,
    string Status,
    int NodeCount
);

public record WorkflowDetailDto(
    string Id,
    string PublicId,
    string Name,
    string Status,
    string FlowJson,
    List<WorkflowNodeDetailDto> Nodes
);

public record WorkflowNodeDetailDto(
    string Id,
    int NodeType,
    string Name,
    string? BusinessNotes,
    bool IsConfigured,
    string? EndpointUrl,
    string? AuthToken,
    int? RetryPolicyMaxAttempts,
    int? RetryPolicyBackoffCoefficient,
    bool? ContractMappingConvertXmlToJson,
    string? ContractMappingQueryParameters,
    string? ContractMappingRequestMapping,
    string? ContractMappingResponseMapping,
    string? MessageTemplate
);
