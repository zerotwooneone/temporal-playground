namespace TemporalDDD.Domain.WorkflowOrchestration.Nodes;

public sealed class NotificationWorkflowNode : WorkflowNode
{
    public string? MessageTemplate { get; private set; }

    private NotificationWorkflowNode() { }

    private NotificationWorkflowNode(WorkflowNodeId id, string name, string? businessNotes)
        : base(id, NodeType.Notification, name, businessNotes)
    {
    }

    internal static NotificationWorkflowNode CreateStub(string name, string? businessNotes)
    {
        return new NotificationWorkflowNode(WorkflowNodeId.New(), name, businessNotes);
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
