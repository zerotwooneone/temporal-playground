using TemporalDDD.Domain.WorkflowOrchestration.ValueObjects;

namespace TemporalDDD.Domain.WorkflowOrchestration.Nodes;

public sealed class HumanTaskWorkflowNode : WorkflowNode
{
    public TaskRole RequiredRole { get; private set; }
    public TemporalSignalName SignalName { get; private set; }
    public TaskTimeout Timeout { get; private set; }
    public string? UIFormSchema { get; private set; }

    private HumanTaskWorkflowNode(WorkflowNodeId id, string name, string? businessNotes)
        : base(id, NodeType.HumanTask, name, businessNotes)
    {
        _outputPorts.Add("Default");
    }

    // Internal constructor for infrastructure rehydration
    internal HumanTaskWorkflowNode(
        WorkflowNodeId id,
        string name,
        string? businessNotes,
        bool isConfigured,
        TaskRole requiredRole,
        TemporalSignalName signalName,
        TaskTimeout timeout,
        string? uiFormSchema,
        IEnumerable<NodeInputDefinition>? inputDefinitions,
        IEnumerable<NodeOutputDefinition>? outputDefinitions)
        : base(id, NodeType.HumanTask, name, businessNotes)
    {
        IsConfigured = isConfigured;
        _outputPorts.Add("Default");
        RequiredRole = requiredRole;
        SignalName = signalName;
        Timeout = timeout;
        UIFormSchema = uiFormSchema;
        if (inputDefinitions != null)
        {
            _inputDefinitions.AddRange(inputDefinitions);
        }
        if (outputDefinitions != null)
        {
            _outputDefinitions.AddRange(outputDefinitions);
        }
    }

    public static HumanTaskWorkflowNode CreateStub(string name, string? businessNotes, WorkflowNodeId? id = null)
    {
        var node = new HumanTaskWorkflowNode(id ?? WorkflowNodeId.New(), name, businessNotes);
        node._outputPorts.Add("Default");
        return node;
    }

    public void ConfigureTechnicalDetails(TaskRole requiredRole, TemporalSignalName signalName, TaskTimeout timeout, string? uiFormSchema)
    {
        RequiredRole = requiredRole;
        SignalName = signalName;
        Timeout = timeout;
        UIFormSchema = uiFormSchema;
        ValidateConfiguration([]);
    }

    public void ConfigureInputs(IEnumerable<NodeInputDefinition> inputs)
    {
        _inputDefinitions.Clear();
        _inputDefinitions.AddRange(inputs);
    }

    public void ConfigureOutputs(IEnumerable<NodeOutputDefinition> outputs)
    {
        _outputDefinitions.Clear();
        _outputDefinitions.AddRange(outputs);
    }

    public override void ValidateConfiguration(IReadOnlyList<WorkflowTransition> transitions)
    {
        // Transitions parameter is not used for HumanTask node configuration
        IsConfigured = RequiredRole != null && SignalName != null && Timeout != null;
    }
}
