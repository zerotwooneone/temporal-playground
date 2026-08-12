namespace TemporalDDD.Infrastructure.WorkflowOrchestration;

public class ApiWorkflowNodeDbo : WorkflowNodeDbo
{
    // Flattened RetryPolicy Value Object
    public int? RetryPolicyMaxAttempts { get; set; }
    public int? RetryPolicyBackoffCoefficient { get; set; }

    // Flattened ContractMapping Value Object
    public bool? ContractMappingConvertXmlToJson { get; set; }
    public string? ContractMappingQueryParameters { get; set; }
    public string? ContractMappingRequestMapping { get; set; }
    public string? ContractMappingResponseMapping { get; set; }

    // Unified technical inputs as JSON
    public string? TechnicalInputsJson { get; set; }
}
