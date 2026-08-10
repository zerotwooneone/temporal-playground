using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TemporalDDD.Domain.IdentityAndAccess;
using TemporalDDD.Domain.WorkflowOrchestration;
using TemporalDDD.Domain.WorkflowOrchestration.Nodes;
using TemporalDDD.Domain.WorkflowOrchestration.ValueObjects;
using TemporalDDD.Infrastructure.Generators;
using Xunit;

namespace TemporalDDD.Infrastructure.Generators.Tests;

public class RoslynWorkflowCodeGeneratorServiceTests
{
    [Fact]
    public async Task GenerateWorkflowClassAsync_WhenValidWorkflowDefinition_GeneratesValidCSharpCode()
    {
        // ARRANGE
        var service = new RoslynWorkflowCodeGeneratorService();
        var userId = UserId.New();
        var publicId = WorkflowDefinitionPublicId.New();
        var className = WorkflowClassName.Create("CandidateOnboarding", publicId).Value;
        
        // Create workflow definition with Start, Api, HumanTask, and End nodes
        var workflowDefinition = new WorkflowDefinition(
            WorkflowDefinitionId.New(),
            publicId,
            userId,
            "Candidate Onboarding",
            className,
            WorkflowStatus.Draft,
            "{}",
            new List<WorkflowNode>
            {
                // Start node
                StartWorkflowNode.CreateStub("Start", null),
                
                // API node - using stub and configuring technical details
                ApiWorkflowNode.CreateStub("Verify Credentials", "Call external API to verify credentials"),
                
                // Human Task node - using stub
                HumanTaskWorkflowNode.CreateStub("Manager Approval", "Requires manager approval"),
                
                // End node
                EndWorkflowNode.CreateStub("End", null)
            });

        // Configure the nodes with technical details
        var startNode = workflowDefinition.Nodes.First(n => n.Name == "Start");
        startNode.UpdateBusinessIntent("Start", null);
        
        var apiNode = (ApiWorkflowNode)workflowDefinition.Nodes.First(n => n.Name == "Verify Credentials");
        apiNode.ConfigureTechnicalDetails(
            "https://api.example.com/verify",
            "token123",
            RetryPolicy.Create(3, 2).Value,
            ContractMapping.Create(true, "param1", "requestMapping", "responseMapping").Value);
        
        var humanTaskNode = (HumanTaskWorkflowNode)workflowDefinition.Nodes.First(n => n.Name == "Manager Approval");
        humanTaskNode.ConfigureTechnicalDetails(
            TaskRole.Create("Manager").Value,
            TemporalSignalName.Create("ManagerApprovalSignal").Value,
            TaskTimeout.Create(60).Value,
            "{ \"schema\": \"approval-form\" }");
        
        var endNode = workflowDefinition.Nodes.First(n => n.Name == "End");
        endNode.UpdateBusinessIntent("End", null);

        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDirectory);

        try
        {
            // ACT
            var generatedCode = await service.GenerateWorkflowClassAsync(workflowDefinition, tempDirectory);

            // ASSERT
            // Use Roslyn to validate the generated code has no syntax errors
            var syntaxTree = CSharpSyntaxTree.ParseText(generatedCode);
            var diagnostics = syntaxTree.GetDiagnostics();
            
            Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);
            Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Warning);

            // Assert that the code contains the expected attributes
            Assert.Contains("[Workflow]", generatedCode);
            Assert.Contains("[WorkflowRun]", generatedCode);
            Assert.Contains("CandidateOnboarding", generatedCode);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
        }
    }
}
