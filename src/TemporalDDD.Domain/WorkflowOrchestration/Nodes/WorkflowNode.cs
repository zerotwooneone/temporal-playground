namespace TemporalDDD.Domain.WorkflowOrchestration.Nodes;

using TemporalDDD.Domain.WorkflowOrchestration;

public abstract class WorkflowNode
{
    public WorkflowNodeId Id { get; private set; }
    public NodeType Type { get; private set; }
    public string Name { get; private set; }
    public string? BusinessNotes { get; private set; }
    public bool IsConfigured { get; protected set; }

    protected readonly List<NodeInputDefinition> _inputDefinitions = new();
    public IReadOnlyList<NodeInputDefinition> InputDefinitions => _inputDefinitions.AsReadOnly();

    protected readonly List<NodeOutputDefinition> _outputDefinitions = new();
    public IReadOnlyList<NodeOutputDefinition> OutputDefinitions => _outputDefinitions.AsReadOnly();

    protected readonly List<string> _outputPorts = new();
    public IReadOnlyList<string> OutputPorts => _outputPorts.AsReadOnly();

    private readonly List<ParameterBinding> _inputBindings = new();
    public IReadOnlyList<ParameterBinding> InputBindings => _inputBindings.AsReadOnly();

    // Unified technical inputs collection
    protected readonly Dictionary<string, InputValueSource> _technicalInputs = new();
    public IReadOnlyDictionary<string, InputValueSource> TechnicalInputs => _technicalInputs;

    public void SetTechnicalInput(string key, InputValueSource value)
    {
        _technicalInputs[key] = value;
        ValidateConfiguration([]);
    }
    public InputValueSource? GetTechnicalInput(string key) => _technicalInputs.TryGetValue(key, out var val) ? val : null;

    protected WorkflowNode() { }

    protected WorkflowNode(WorkflowNodeId id, NodeType type, string name, string? businessNotes)
    {
        Id = id;
        Type = type;
        Name = name;
        BusinessNotes = businessNotes;
        IsConfigured = false;
    }

    public void UpdateBusinessIntent(string name, string? businessNotes)
    {
        Name = name;
        BusinessNotes = businessNotes;
    }

    public void UpdateInputBindings(IEnumerable<ParameterBinding> newBindings)
    {
        _inputBindings.Clear();
        _inputBindings.AddRange(newBindings);
    }

    public abstract void ValidateConfiguration(IReadOnlyList<WorkflowTransition> transitions);
}
