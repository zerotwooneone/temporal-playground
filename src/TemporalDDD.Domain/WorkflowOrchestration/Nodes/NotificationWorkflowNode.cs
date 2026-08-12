namespace TemporalDDD.Domain.WorkflowOrchestration.Nodes;

using TemporalDDD.Domain.WorkflowOrchestration;

public sealed class NotificationWorkflowNode : WorkflowNode
{
    public const string MessageTemplateKey = "MessageTemplate";

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
        Dictionary<string, InputValueSource> technicalInputs,
        IEnumerable<NodeInputDefinition>? inputDefinitions,
        IEnumerable<NodeOutputDefinition>? outputDefinitions)
        : base(id, NodeType.Notification, name, businessNotes)
    {
        IsConfigured = isConfigured;
        foreach (var (key, value) in technicalInputs)
        {
            _technicalInputs[key] = value;
        }
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

    public static NotificationWorkflowNode CreateStub(string name, string? businessNotes)
    {
        var node = new NotificationWorkflowNode(WorkflowNodeId.New(), name, businessNotes);
        node._inputDefinitions.Add(new NodeInputDefinition("MessageTemplate", WorkflowDataType.Primitive.String, true));
        // Outputs remain empty because it is a sink/pass-through
        return node;
    }

    public override void ValidateConfiguration()
    {
        if (!_technicalInputs.ContainsKey(MessageTemplateKey))
        {
            IsConfigured = false;
            return;
        }

        var messageTemplate = GetTechnicalInput(MessageTemplateKey);
        
        // If Fixed, check that the value is not whitespace
        if (messageTemplate is InputValueSource.Fixed fixedValue)
        {
            IsConfigured = !string.IsNullOrWhiteSpace(fixedValue.Value);
        }
        else
        {
            // Mapped is always considered configured (validation happens at workflow level)
            IsConfigured = true;
        }
    }
}
