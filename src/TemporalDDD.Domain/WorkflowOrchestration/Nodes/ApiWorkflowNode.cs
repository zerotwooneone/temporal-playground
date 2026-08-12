using TemporalDDD.Domain.WorkflowOrchestration.ValueObjects;

namespace TemporalDDD.Domain.WorkflowOrchestration.Nodes;

public sealed class ApiWorkflowNode : WorkflowNode, IActivityWorkflowNode
{
    public RetryPolicy? RetryPolicy { get; private set; }
    public ContractMapping? ContractMapping { get; private set; }

    private ApiWorkflowNode(WorkflowNodeId id, string name, string? businessNotes)
        : base(id, NodeType.Api, name, businessNotes)
    {
    }

    // Internal constructor for infrastructure rehydration
    internal ApiWorkflowNode(
        WorkflowNodeId id,
        string name,
        string? businessNotes,
        bool isConfigured,
        Dictionary<string, InputValueSource> technicalInputs,
        RetryPolicy? retryPolicy,
        ContractMapping? contractMapping,
        IEnumerable<NodeInputDefinition>? inputDefinitions,
        IEnumerable<NodeOutputDefinition>? outputDefinitions)
        : base(id, NodeType.Api, name, businessNotes)
    {
        IsConfigured = isConfigured;
        foreach (var (key, value) in technicalInputs)
        {
            _technicalInputs[key] = value;
        }
        RetryPolicy = retryPolicy;
        ContractMapping = contractMapping;
        if (inputDefinitions != null)
        {
            _inputDefinitions.AddRange(inputDefinitions);
        }
        if (outputDefinitions != null)
        {
            _outputDefinitions.AddRange(outputDefinitions);
        }
        // Re-validate after rehydration
        ValidateConfiguration();
    }

    public static ApiWorkflowNode CreateStub(string name, string? businessNotes)
    {
        var node = new ApiWorkflowNode(WorkflowNodeId.New(), name, businessNotes);
        // Fixed output: API nodes always return a JsonDocument
        node._outputDefinitions.Add(new NodeOutputDefinition("ApiResponse", WorkflowDataType.Primitive.Json));
        return node;
    }

    public void ConfigureValueObjects(RetryPolicy retryPolicy, ContractMapping mapping)
    {
        RetryPolicy = retryPolicy;
        ContractMapping = mapping;
        ValidateConfiguration();
    }

    public override void ValidateConfiguration()
    {
        IsConfigured = _technicalInputs.ContainsKey("EndpointUrl") && RetryPolicy != null && ContractMapping != null;
    }

    // Helper methods for backward compatibility / query layer
    public string? GetFixedEndpointUrl() => 
        GetTechnicalInput("EndpointUrl") is InputValueSource.Fixed fixedValue ? fixedValue.Value : null;

    public string? GetFixedAuthToken() => 
        GetTechnicalInput("AuthToken") is InputValueSource.Fixed fixedValue ? fixedValue.Value : null;
}
