using TemporalDDD.Domain.WorkflowOrchestration.ValueObjects;

namespace TemporalDDD.Domain.WorkflowOrchestration.Nodes;

public sealed class ApiWorkflowNode : WorkflowNode, IActivityWorkflowNode
{
    public const string EndpointUrlKey = "EndpointUrl";
    public const string AuthTokenKey = "AuthToken";

    public RetryPolicy? RetryPolicy { get; private set; }
    public ContractMapping? ContractMapping { get; private set; }

    private ApiWorkflowNode(WorkflowNodeId id, string name, string? businessNotes)
        : base(id, NodeType.Api, name, businessNotes)
    {
        _outputPorts.Add("Default");
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
        _outputPorts.Add("Default");
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
        node._outputPorts.Add("Default");
        // Technical input definitions: EndpointUrl (Required, String) and AuthToken (Optional, String)
        node._inputDefinitions.Add(new NodeInputDefinition(EndpointUrlKey, WorkflowDataType.Primitive.String, true));
        node._inputDefinitions.Add(new NodeInputDefinition(AuthTokenKey, WorkflowDataType.Primitive.String, false));
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
        IsConfigured = _technicalInputs.ContainsKey(EndpointUrlKey) && RetryPolicy != null && ContractMapping != null;
    }
}
