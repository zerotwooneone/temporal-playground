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
        ContractMapping? contractMapping)
        : base(id, NodeType.Api, name, businessNotes)
    {
        IsConfigured = isConfigured;
        EndpointUrl = endpointUrl;
        AuthToken = authToken;
        RetryPolicy = retryPolicy;
        ContractMapping = contractMapping;
    }

    public static ApiWorkflowNode CreateStub(string name, string? businessNotes)
    {
        return new ApiWorkflowNode(WorkflowNodeId.New(), name, businessNotes);
    }

    public void ConfigureTechnicalDetails(string endpointUrl, string? authToken, RetryPolicy retryPolicy, ContractMapping mapping)
    {
        EndpointUrl = endpointUrl;
        AuthToken = authToken;
        RetryPolicy = retryPolicy;
        ContractMapping = mapping;
        ValidateConfiguration();
    }

    public void ConfigureResponseSchema(IEnumerable<NodeOutputDefinition> schemaOutputs)
    {
        // The API output is locked in when the user configures the node
        _outputDefinitions.Clear();
        _outputDefinitions.AddRange(schemaOutputs);
    }

    public override void ValidateConfiguration()
    {
        IsConfigured = EndpointUrl != null && RetryPolicy != null && ContractMapping != null;
    }
}
