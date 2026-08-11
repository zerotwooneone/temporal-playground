using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TemporalDDD.Infrastructure.WorkflowOrchestration;

public class WorkflowInstanceConfiguration : IEntityTypeConfiguration<WorkflowInstanceDbo>
{
    public void Configure(EntityTypeBuilder<WorkflowInstanceDbo> builder)
    {
        builder.ToTable("Workflow_Instances");

        // Primary Key - internal ID (V7 Guid)
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .IsRequired();

        // Public ID - Alternate Key (V4 Guid) for API lookups
        builder.HasAlternateKey(x => x.PublicId);
        builder.Property(x => x.PublicId)
            .IsRequired();

        // Foreign Key to WorkflowDefinition
        builder.Property(x => x.WorkflowDefinitionId)
            .IsRequired();

        // Business Reference ID
        builder.Property(x => x.BusinessReferenceId)
            .IsRequired();

        // Temporal Run ID
        builder.Property(x => x.TemporalRunId)
            .IsRequired();

        // Status enum - stored as int
        builder.Property(x => x.Status)
            .IsRequired();

        // WorkflowContextData Value Object - stored as JSON string
        builder.Property(x => x.ContextData)
            .IsRequired();
    }
}
