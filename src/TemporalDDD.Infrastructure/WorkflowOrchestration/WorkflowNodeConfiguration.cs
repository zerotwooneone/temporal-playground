using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TemporalDDD.Infrastructure.WorkflowOrchestration;

public class WorkflowNodeConfiguration : IEntityTypeConfiguration<WorkflowNodeDbo>
{
    public void Configure(EntityTypeBuilder<WorkflowNodeDbo> builder)
    {
        builder.ToTable("Workflow_Nodes");

        // Primary Key - client-generated string ID
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .IsRequired();

        // Foreign Key to WorkflowDefinition
        builder.Property(x => x.WorkflowDefinitionId)
            .IsRequired();

        // Smart Enum - stored as int
        builder.Property(x => x.NodeType)
            .IsRequired();

        // Other properties
        builder.Property(x => x.Name)
            .IsRequired();

        builder.Property(x => x.BusinessNotes)
            .IsRequired(false);

        builder.Property(x => x.IsConfigured)
            .IsRequired();

        // Table-Per-Hierarchy (TPH) using NodeType as discriminator
        builder.HasDiscriminator(x => x.NodeType)
            .HasValue<ApiWorkflowNodeDbo>(1)  // NodeType.Api
            .HasValue<NotificationWorkflowNodeDbo>(2);  // NodeType.Notification
    }
}

public class ApiWorkflowNodeConfiguration : IEntityTypeConfiguration<ApiWorkflowNodeDbo>
{
    public void Configure(EntityTypeBuilder<ApiWorkflowNodeDbo> builder)
    {
        // Configure ApiWorkflowNodeDbo specific properties
        builder.Property(x => x.EndpointUrl)
            .IsRequired(false);

        builder.Property(x => x.AuthToken)
            .IsRequired(false);

        // Flattened RetryPolicy properties
        builder.Property(x => x.RetryPolicyMaxAttempts)
            .IsRequired(false);

        builder.Property(x => x.RetryPolicyBackoffCoefficient)
            .IsRequired(false);

        // Flattened ContractMapping properties
        builder.Property(x => x.ContractMappingConvertXmlToJson)
            .IsRequired(false);

        builder.Property(x => x.ContractMappingQueryParameters)
            .IsRequired(false);

        builder.Property(x => x.ContractMappingRequestMapping)
            .IsRequired(false);

        builder.Property(x => x.ContractMappingResponseMapping)
            .IsRequired(false);
    }
}

public class NotificationWorkflowNodeConfiguration : IEntityTypeConfiguration<NotificationWorkflowNodeDbo>
{
    public void Configure(EntityTypeBuilder<NotificationWorkflowNodeDbo> builder)
    {
        // Configure NotificationWorkflowNodeDbo specific properties
        builder.Property(x => x.MessageTemplate)
            .IsRequired(false);
    }
}
