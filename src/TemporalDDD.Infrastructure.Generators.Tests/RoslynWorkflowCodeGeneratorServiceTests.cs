using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using TemporalDDD.Domain.IdentityAndAccess;
using TemporalDDD.Domain.WorkflowOrchestration;
using TemporalDDD.Domain.WorkflowOrchestration.Nodes;
using TemporalDDD.Domain.WorkflowOrchestration.ValueObjects;

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
        apiNode.SetTechnicalInput(ApiWorkflowNode.EndpointUrlKey, new InputValueSource.Fixed("https://api.example.com/verify"));
        apiNode.SetTechnicalInput(ApiWorkflowNode.AuthTokenKey, new InputValueSource.Fixed("token123"));
        apiNode.ConfigureValueObjects(
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
            new List<WorkflowNode> { startNode, apiNode, humanTaskNode, endNode },
            transitions);

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

    [Fact]
    public async Task GenerateWorkflowClassAsync_WhenNoStartNode_ThrowsInvalidOperationException()
    {
        // ARRANGE
        var service = new RoslynWorkflowCodeGeneratorService();
        var userId = UserId.New();
        var publicId = WorkflowDefinitionPublicId.New();
        var className = WorkflowClassName.Create("TestWorkflow", publicId).Value;
        
        // Create nodes without a Start node
        var apiNode = ApiWorkflowNode.CreateStub("API Node", null);
        var endNode = EndWorkflowNode.CreateStub("End", null);

        var transitions = new List<WorkflowTransition>
        {
            new WorkflowTransition(apiNode.Id, endNode.Id)
        };

        var workflowDefinition = new WorkflowDefinition(
            WorkflowDefinitionId.New(),
            publicId,
            userId,
            "Test Workflow",
            className,
            WorkflowStatus.Draft,
            "{}",
            new List<WorkflowNode> { apiNode, endNode },
            transitions);

        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDirectory);

        try
        {
            // ACT
            var action = async () => await service.GenerateWorkflowClassAsync(workflowDefinition, tempDirectory);

            // ASSERT
            await Assert.ThrowsAsync<InvalidOperationException>(action);
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

    #region SanitizeIdentifier Tests
    [Theory]
    [InlineData("ValidName", "ValidName")]
    [InlineData("Valid_Name_123", "Valid_Name_123")]
    [InlineData("Invalid@Name", "InvalidName")]
    [InlineData("Invalid#Name", "InvalidName")]
    [InlineData("Invalid Name", "InvalidName")]
    [InlineData("123Invalid", "_123Invalid")]
    [InlineData("", "_")]
    [InlineData("a", "a")]
    [InlineData("_", "_")]
    [InlineData("___", "___")]
    public void SanitizeIdentifier_WithVariousInputs_ReturnsValidCSharpIdentifier(string input, string expected)
    {
        // ACT
        var result = RoslynWorkflowCodeGeneratorService.SanitizeIdentifier(input);

        // ASSERT
        Assert.Equal(expected, result);
    }
    #endregion

    [Fact]
    public async Task GenerateWorkflowClassAsync_WithParallelBranches_GeneratesTaskWhenAll()
    {
        // ARRANGE
        var service = new RoslynWorkflowCodeGeneratorService();
        var userId = UserId.New();
        var publicId = WorkflowDefinitionPublicId.New();
        var className = WorkflowClassName.Create("ParallelWorkflow", publicId).Value;
        
        // Create nodes with parallel branches: Start -> (API1, API2) -> End
        var startNode = StartWorkflowNode.CreateStub("Start", null);
        var apiNode1 = ApiWorkflowNode.CreateStub("API 1", null);
        var apiNode2 = ApiWorkflowNode.CreateStub("API 2", null);
        var endNode = EndWorkflowNode.CreateStub("End", null);

        // Configure API nodes
        apiNode1.SetTechnicalInput(ApiWorkflowNode.EndpointUrlKey, new InputValueSource.Fixed("https://api1.example.com"));
        apiNode1.SetTechnicalInput(ApiWorkflowNode.AuthTokenKey, new InputValueSource.Fixed("token1"));
        apiNode1.ConfigureValueObjects(
            RetryPolicy.Create(3, 2).Value,
            ContractMapping.Create(false, null, null, null).Value);
        apiNode2.SetTechnicalInput(ApiWorkflowNode.EndpointUrlKey, new InputValueSource.Fixed("https://api2.example.com"));
        apiNode2.SetTechnicalInput(ApiWorkflowNode.AuthTokenKey, new InputValueSource.Fixed("token2"));
        apiNode2.ConfigureValueObjects(
            RetryPolicy.Create(3, 2).Value,
            ContractMapping.Create(false, null, null, null).Value);

        // Create transitions with parallel branches
        var transitions = new List<WorkflowTransition>
        {
            new WorkflowTransition(startNode.Id, apiNode1.Id),
            new WorkflowTransition(startNode.Id, apiNode2.Id),
            new WorkflowTransition(apiNode1.Id, endNode.Id),
            new WorkflowTransition(apiNode2.Id, endNode.Id)
        };

        var workflowDefinition = new WorkflowDefinition(
            WorkflowDefinitionId.New(),
            publicId,
            userId,
            "Parallel Workflow",
            className,
            WorkflowStatus.Draft,
            "{}",
            new List<WorkflowNode> { startNode, apiNode1, apiNode2, endNode },
            transitions);

        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDirectory);

        try
        {
            // ACT
            var generatedCode = await service.GenerateWorkflowClassAsync(workflowDefinition, tempDirectory);

            // ASSERT
            Assert.Contains("Task.WhenAll", generatedCode);
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

    [Fact]
    public async Task GenerateWorkflowClassAsync_WithApiNode_GeneratesExecuteApiCall()
    {
        // ARRANGE
        var service = new RoslynWorkflowCodeGeneratorService();
        var userId = UserId.New();
        var publicId = WorkflowDefinitionPublicId.New();
        var className = WorkflowClassName.Create("ApiWorkflow", publicId).Value;
        
        var startNode = StartWorkflowNode.CreateStub("Start", null);
        var apiNode = ApiWorkflowNode.CreateStub("API Node", null);
        var endNode = EndWorkflowNode.CreateStub("End", null);

        apiNode.SetTechnicalInput(ApiWorkflowNode.EndpointUrlKey, new InputValueSource.Fixed("https://api.example.com"));
        apiNode.SetTechnicalInput(ApiWorkflowNode.AuthTokenKey, new InputValueSource.Fixed("token"));
        apiNode.ConfigureValueObjects(
            RetryPolicy.Create(3, 2).Value,
            ContractMapping.Create(false, null, null, null).Value);

        var transitions = new List<WorkflowTransition>
        {
            new WorkflowTransition(startNode.Id, apiNode.Id),
            new WorkflowTransition(apiNode.Id, endNode.Id)
        };

        var workflowDefinition = new WorkflowDefinition(
            WorkflowDefinitionId.New(),
            publicId,
            userId,
            "API Workflow",
            className,
            WorkflowStatus.Draft,
            "{}",
            new List<WorkflowNode> { startNode, apiNode, endNode },
            transitions);

        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDirectory);

        try
        {
            // ACT
            var generatedCode = await service.GenerateWorkflowClassAsync(workflowDefinition, tempDirectory);

            // ASSERT
            Assert.Contains("ExecuteApiCallAsync", generatedCode);
            Assert.Contains("ExecuteApiInput", generatedCode);
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

    [Fact]
    public async Task GenerateWorkflowClassAsync_WithNotificationNode_GeneratesSendNotification()
    {
        // ARRANGE
        var service = new RoslynWorkflowCodeGeneratorService();
        var userId = UserId.New();
        var publicId = WorkflowDefinitionPublicId.New();
        var className = WorkflowClassName.Create("NotificationWorkflow", publicId).Value;
        
        var startNode = StartWorkflowNode.CreateStub("Start", null);
        var notificationNode = NotificationWorkflowNode.CreateStub("Notification", null);
        var endNode = EndWorkflowNode.CreateStub("End", null);

        notificationNode.SetTechnicalInput(NotificationWorkflowNode.MessageTemplateKey, new InputValueSource.Fixed("Hello {name}"));

        var transitions = new List<WorkflowTransition>
        {
            new WorkflowTransition(startNode.Id, notificationNode.Id),
            new WorkflowTransition(notificationNode.Id, endNode.Id)
        };

        var workflowDefinition = new WorkflowDefinition(
            WorkflowDefinitionId.New(),
            publicId,
            userId,
            "Notification Workflow",
            className,
            WorkflowStatus.Draft,
            "{}",
            new List<WorkflowNode> { startNode, notificationNode, endNode },
            transitions);

        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDirectory);

        try
        {
            // ACT
            var generatedCode = await service.GenerateWorkflowClassAsync(workflowDefinition, tempDirectory);

            // ASSERT
            Assert.Contains("SendNotificationAsync", generatedCode);
            Assert.Contains("SendNotificationInput", generatedCode);
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

    [Fact]
    public async Task GenerateWorkflowClassAsync_WritesFileWithCorrectName()
    {
        // ARRANGE
        var service = new RoslynWorkflowCodeGeneratorService();
        var userId = UserId.New();
        var publicId = WorkflowDefinitionPublicId.New();
        var className = WorkflowClassName.Create("FileOutputTest", publicId).Value;
        
        var startNode = StartWorkflowNode.CreateStub("Start", null);
        var endNode = EndWorkflowNode.CreateStub("End", null);

        var transitions = new List<WorkflowTransition>
        {
            new WorkflowTransition(startNode.Id, endNode.Id)
        };

        var workflowDefinition = new WorkflowDefinition(
            WorkflowDefinitionId.New(),
            publicId,
            userId,
            "File Output Test",
            className,
            WorkflowStatus.Draft,
            "{}",
            new List<WorkflowNode> { startNode, endNode },
            transitions);

        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDirectory);

        try
        {
            // ACT
            await service.GenerateWorkflowClassAsync(workflowDefinition, tempDirectory);

            // ASSERT
            var expectedFilePath = Path.Combine(tempDirectory, $"{className.Value}.cs");
            Assert.True(File.Exists(expectedFilePath));
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
