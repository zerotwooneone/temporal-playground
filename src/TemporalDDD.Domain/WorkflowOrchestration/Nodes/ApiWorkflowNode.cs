using TemporalDDD.Domain.WorkflowOrchestration.ValueObjects;

namespace TemporalDDD.Domain.WorkflowOrchestration.Nodes;

public sealed class ApiWorkflowNode : WorkflowNode, IActivityWorkflowNode
{
    public string? EndpointUrl { get; private set; }
    public string? AuthToken { get; private set; }
    public RetryPolicy? RetryPolicy { get; private set; }
    public ContractMapping? ContractMapping { get; private set; }

    private ApiWorkflowNode() { }

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
        string? endpointUrl,
        string? authToken,
        RetryPolicy? retryPolicy,
        ContractMapping? contractMapping,
        IEnumerable<NodeInputDefinition>? inputDefinitions,
        IEnumerable<NodeOutputDefinition>? outputDefinitions)
        : base(id, NodeType.Api, name, businessNotes)
    {
        IsConfigured = isConfigured;
        EndpointUrl = endpointUrl;
        AuthToken = authToken;
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
    }

    public static ApiWorkflowNode CreateStub(string name, string? businessNotes)
    {
        var node = new ApiWorkflowNode(WorkflowNodeId.New(), name, businessNotes);
        // Fixed output: API nodes always return a JsonDocument
        node._outputDefinitions.Add(new NodeOutputDefinition("ApiResponse", WorkflowDataType.Primitive.Json));
        return node;
    }

    public void ConfigureTechnicalDetails(string endpointUrl, string? authToken, RetryPolicy retryPolicy, ContractMapping mapping)
    {
        EndpointUrl = endpointUrl;
        AuthToken = authToken;
        RetryPolicy = retryPolicy;
        ContractMapping = mapping;
        ValidateConfiguration();
    }

    public override void ValidateConfiguration()
    {
        IsConfigured = EndpointUrl != null && RetryPolicy != null && ContractMapping != null;
    }
}
