using Microsoft.EntityFrameworkCore;
using TemporalDDD.Application.WorkflowOrchestration;
using TemporalDDD.Domain.WorkflowOrchestration;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.WorkflowOrchestration;

public class WorkflowDefinitionQuery : IWorkflowDefinitionQuery
{
    private readonly ApplicationDbContext _dbContext;

    public WorkflowDefinitionQuery(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<WorkflowDefinitionDto>> GetAllWorkflowsAsync(CancellationToken cancellationToken = default)
    {
        var workflows = await _dbContext.WorkflowDefinitions
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var result = new List<WorkflowDefinitionDto>();

        foreach (var dbo in workflows)
        {
            var statusResult = WorkflowStatus.FromValue(dbo.Status);
            if (statusResult.IsFailure)
                continue; // Skip invalid entries

            var nodeCount = await _dbContext.WorkflowNodes
                .AsNoTracking()
                .CountAsync(n => n.WorkflowDefinitionId == dbo.Id, cancellationToken);

            result.Add(new WorkflowDefinitionDto(
                Id: dbo.Id,
                PublicId: dbo.PublicId,
                Name: dbo.Name,
                Status: statusResult.Value.Name,
                NodeCount: nodeCount
            ));
        }

        return result.AsReadOnly();
    }
}
