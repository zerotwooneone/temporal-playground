using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
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

            // Parse the text into a syntax tree
            var syntaxTree = CSharpSyntaxTree.ParseText(generatedCode);

            // Create a compilation and add necessary metadata references
            var references = new List<MetadataReference>();

            // Add all currently loaded assemblies
            references.AddRange(AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
                .Select(assembly => MetadataReference.CreateFromFile(assembly.Location)));

            // Explicitly add Temporalio assemblies by loading them first
            var temporalioAssemblies = new[] { "Temporalio", "Temporalio.Client", "Temporalio.Workflows" };
            foreach (var assemblyName in temporalioAssemblies)
            {
                try
                {
                    var assembly = Assembly.Load(assemblyName);
                    if (!string.IsNullOrEmpty(assembly.Location))
                    {
                        references.Add(MetadataReference.CreateFromFile(assembly.Location));
                    }
                }
                catch
                {
                    // Assembly not found, skip
                }
            }

            // Add System.Text.Json if not already loaded
            try
            {
                var jsonAssembly = Assembly.Load("System.Text.Json");
                if (!string.IsNullOrEmpty(jsonAssembly.Location))
                {
                    references.Add(MetadataReference.CreateFromFile(jsonAssembly.Location));
                }
            }
            catch
            {
                // Assembly not found, skip
            }

            // Add explicit references to project assemblies
            var infrastructureAssembly = Assembly.Load("TemporalDDD.Infrastructure");
            if (!string.IsNullOrEmpty(infrastructureAssembly.Location))
            {
                references.Add(MetadataReference.CreateFromFile(infrastructureAssembly.Location));
            }

            var domainAssembly = Assembly.Load("TemporalDDD.Domain");
            if (!string.IsNullOrEmpty(domainAssembly.Location))
            {
                references.Add(MetadataReference.CreateFromFile(domainAssembly.Location));
            }

            var compilation = CSharpCompilation.Create(
                assemblyName: "DynamicWorkflowTestAssembly",
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            // Emit to a memory stream to trigger full semantic analysis and type-checking
            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            // ASSERT
            // Assert that there are zero actual compilation errors
            var compilationErrors = result.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList();

            Assert.Empty(compilationErrors);

            // Additional assertions for code structure
            Assert.Contains("[Workflow]", generatedCode);
            Assert.Contains("[WorkflowRun]", generatedCode);
            Assert.Contains("CandidateOnboarding", generatedCode);
            Assert.Contains("ExecuteApiCallAsync", generatedCode);
            Assert.Contains("WaitConditionAsync", generatedCode);
            Assert.Contains("ExecuteActivityAsync", generatedCode);

            // Assert that the code contains the unrolled signal flag
            Assert.Contains("_signal_", generatedCode);
            Assert.Contains("[WorkflowSignal]", generatedCode);
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
