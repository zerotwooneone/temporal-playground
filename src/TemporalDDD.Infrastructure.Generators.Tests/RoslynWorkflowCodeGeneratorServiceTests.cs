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
        
        // Create nodes
        var startNode = StartWorkflowNode.CreateStub("Start", null);
        var apiNode = ApiWorkflowNode.CreateStub("Verify Credentials", "Call external API to verify credentials");
        var humanTaskNode = HumanTaskWorkflowNode.CreateStub("Manager Approval", "Requires manager approval");
        var endNode = EndWorkflowNode.CreateStub("End", null);

        // Configure the nodes with technical details
        apiNode.ConfigureTechnicalDetails(
            "https://api.example.com/verify",
            "token123",
            RetryPolicy.Create(3, 2).Value,
            ContractMapping.Create(true, "param1", "requestMapping", "responseMapping").Value);
        
        humanTaskNode.ConfigureTechnicalDetails(
            TaskRole.Create("Manager").Value,
            TemporalSignalName.Create("ManagerApprovalSignal").Value,
            TaskTimeout.Create(60).Value,
            "{ \"schema\": \"approval-form\" }");

        // Create transitions: Start -> Api -> HumanTask -> End
        var transitions = new List<WorkflowTransition>
        {
            new WorkflowTransition(startNode.Id, apiNode.Id),
            new WorkflowTransition(apiNode.Id, humanTaskNode.Id),
            new WorkflowTransition(humanTaskNode.Id, endNode.Id)
        };

        // Create workflow definition with nodes and transitions
        var workflowDefinition = new WorkflowDefinition(
            WorkflowDefinitionId.New(),
            publicId,
            userId,
            "Candidate Onboarding",
            className,
            WorkflowStatus.Draft,
            "{}",
            new List<WorkflowNode> { startNode, apiNode, humanTaskNode, endNode });

        // Update the workflow with transitions
        workflowDefinition.UpdateNodes(
            new List<WorkflowNode> { startNode, apiNode, humanTaskNode, endNode },
            transitions,
            "{}");

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

            // Assert that the code contains the expected attributes and method calls
            Assert.Contains("[Workflow]", generatedCode);
            Assert.Contains("[WorkflowRun]", generatedCode);
            Assert.Contains("CandidateOnboarding", generatedCode);
            Assert.Contains("ExecuteApiCallAsync", generatedCode);
            Assert.Contains("WaitConditionAsync", generatedCode);
            Assert.Contains("ExecuteActivityAsync", generatedCode);
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
