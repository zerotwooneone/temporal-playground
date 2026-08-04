using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TemporalDDD.Infrastructure.WorkflowOrchestration;

public class WorkflowDefinitionConfiguration : IEntityTypeConfiguration<WorkflowDefinitionDbo>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinitionDbo> builder)
    {
        builder.ToTable("Workflow_Definitions");

        // Primary Key - client-generated string ID
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .IsRequired();

        // PublicId - stored as TEXT with Unique Index
        builder.Property(x => x.PublicId)
            .IsRequired();

        builder.HasIndex(x => x.PublicId)
            .IsUnique();

        // CreatorId - FK to Users
        builder.Property(x => x.CreatorId)
            .IsRequired();

        // Other properties
        builder.Property(x => x.Name)
            .IsRequired();

        // Smart Enum - stored as int
        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.FlowJson)
            .IsRequired();

        // Relationship to WorkflowNodes
        builder.HasMany<WorkflowNodeDbo>()
            .WithOne()
            .HasForeignKey(x => x.WorkflowDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
