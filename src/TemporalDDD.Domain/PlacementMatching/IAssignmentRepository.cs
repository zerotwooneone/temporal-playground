namespace TemporalDDD.Domain.PlacementMatching;

public interface IAssignmentRepository
{
    Task<Assignment?> GetByIdAsync(AssignmentId id, CancellationToken cancellationToken = default);
    Task SaveAsync(Assignment aggregate, CancellationToken cancellationToken = default);
}
