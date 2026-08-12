namespace TemporalDDD.Domain.WorkflowOrchestration;

public abstract record InputValueSource
{
    public sealed record Fixed(string Value) : InputValueSource;
    public sealed record Mapped(VariableReference Source) : InputValueSource;
}
