namespace TemporalDDD.Domain.WorkflowOrchestration.Nodes;

using TemporalDDD.Domain.WorkflowOrchestration;

public sealed class NotificationWorkflowNode : WorkflowNode
{
    public string? MessageTemplate { get; private set; }

    private NotificationWorkflowNode() { }

    private NotificationWorkflowNode(WorkflowNodeId id, string name, string? businessNotes)
        : base(id, NodeType.Notification, name, businessNotes)
    {
    }

    // Internal constructor for infrastructure rehydration
    internal NotificationWorkflowNode(
        WorkflowNodeId id,
        string name,
        string? businessNotes,
        bool isConfigured,
        string? messageTemplate)
        : base(id, NodeType.Notification, name, businessNotes)
    {
        IsConfigured = isConfigured;
        MessageTemplate = messageTemplate;
    }

    public static NotificationWorkflowNode CreateStub(string name, string? businessNotes)
    {
        var node = new NotificationWorkflowNode(WorkflowNodeId.New(), name, businessNotes);
        node._inputDefinitions.Add(new NodeInputDefinition("MessageTemplate", WorkflowDataType.Primitive.String, true));
        // Outputs remain empty because it is a sink/pass-through
        return node;
    }

    public void ConfigureTechnicalDetails(string messageTemplate)
    {
        MessageTemplate = messageTemplate;
        ValidateConfiguration();
    }

    public override void ValidateConfiguration()
    {
        IsConfigured = !string.IsNullOrWhiteSpace(MessageTemplate);
    }
}
