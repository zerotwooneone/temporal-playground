namespace TemporalDDD.Domain.SeedWork;

public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}