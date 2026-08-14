namespace TemporalDDD.Domain.WorkflowOrchestration.Nodes;

using TemporalDDD.Domain.WorkflowOrchestration;

public sealed class StartWorkflowNode : WorkflowNode
{
    private StartWorkflowNode(WorkflowNodeId id, string name, string? businessNotes)
        : base(id, NodeType.Start, name, businessNotes)
    {
        _outputPorts.Add("Default");
    }

    // Internal constructor for infrastructure rehydration
    internal StartWorkflowNode(
        WorkflowNodeId id,
        string name,
        string? businessNotes,
        bool isConfigured,
        IEnumerable<NodeOutputDefinition>? outputDefinitions)
        : base(id, NodeType.Start, name, businessNotes)
    {
        IsConfigured = isConfigured;
        _outputPorts.Add("Default");
        if (outputDefinitions != null)
        {
            _outputDefinitions.AddRange(outputDefinitions);
        }
    }

    public static StartWorkflowNode CreateStub(string name, string? businessNotes)
    {
        var node = new StartWorkflowNode(WorkflowNodeId.New(), name, businessNotes);
        node._outputPorts.Add("Default");
        node.IsConfigured = true; // Start nodes are always configured
        return node;
    }

    public void ConfigureOutputs(IEnumerable<NodeOutputDefinition> workflowInputs)
    {
        _outputDefinitions.Clear();
        _outputDefinitions.AddRange(workflowInputs);
    }

    public override void ValidateConfiguration(IReadOnlyList<WorkflowTransition> transitions)
    {
        // Transitions parameter is not used for Start node configuration
        IsConfigured = true; // Start nodes are always configured
    }
}
