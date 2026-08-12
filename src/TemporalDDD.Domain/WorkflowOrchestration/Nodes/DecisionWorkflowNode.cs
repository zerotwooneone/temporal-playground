namespace TemporalDDD.Domain.WorkflowOrchestration.Nodes;

using TemporalDDD.Domain.WorkflowOrchestration;

public class DecisionWorkflowNode : WorkflowNode
{
    private DecisionWorkflowNode(WorkflowNodeId id, string name, string? businessNotes) 
        : base(id, NodeType.Decision, name, businessNotes)
    {
        // A Decision Node REQUIRES a Boolean input to evaluate
        _inputDefinitions.Add(new NodeInputDefinition("Condition", WorkflowDataType.Primitive.Boolean, true));
    }

    public static DecisionWorkflowNode CreateStub(string name, string? businessNotes)
    {
        return new DecisionWorkflowNode(WorkflowNodeId.New(), name, businessNotes);
    }

    public override void ValidateConfiguration()
    {
        // Decision nodes are always configured once created
        IsConfigured = true;
    }
}
