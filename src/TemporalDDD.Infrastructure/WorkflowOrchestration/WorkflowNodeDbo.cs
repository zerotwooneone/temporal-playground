using TemporalDDD.Domain.WorkflowOrchestration;

namespace TemporalDDD.Infrastructure.WorkflowOrchestration;

public abstract class WorkflowNodeDbo
{
    public string Id { get; set; }
    public string WorkflowDefinitionId { get; set; }
    public int NodeType { get; set; }
    public string Name { get; set; }
    public string? BusinessNotes { get; set; }
    public bool IsConfigured { get; set; }
    
    // JSON columns for Node Contracts and Bindings
    public string InputDefinitionsJson { get; set; } = "[]";
    public string OutputDefinitionsJson { get; set; } = "[]";
    public string InputBindingsJson { get; set; } = "[]";
}
