using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using TemporalDDD.Application.WorkflowOrchestration;
using TemporalDDD.Domain.WorkflowOrchestration;
using TemporalDDD.Domain.WorkflowOrchestration.Nodes;
using TemporalDDD.Domain.WorkflowOrchestration.ValueObjects;
using TemporalDDD.Infrastructure.WorkflowOrchestration.Activities;

namespace TemporalDDD.Infrastructure.Generators;

public class RoslynWorkflowCodeGeneratorService : IWorkflowCodeGeneratorService
{
    public async Task<string> GenerateWorkflowClassAsync(WorkflowDefinition workflowDefinition, string outputDirectoryPath)
    {
        var className = workflowDefinition.ClassName.Value;
        var nodes = workflowDefinition.Nodes.ToDictionary(n => n.Id);
        var transitions = workflowDefinition.Transitions.ToList();

        // Build adjacency list for graph traversal
        var adjacency = transitions
            .GroupBy(t => t.SourceNodeId)
            .ToDictionary(g => g.Key, g => g.Select(t => t.TargetNodeId).ToList());

        // Find start node
        var startNode = workflowDefinition.Nodes.FirstOrDefault(n => n.Type == NodeType.Start);
        if (startNode == null)
            throw new InvalidOperationException("Workflow must have a Start node");

        // Build raw C# code using StringBuilder
        var sb = new StringBuilder();
        sb.AppendLine("using Temporalio.Workflows;");
        sb.AppendLine("using Temporalio.Activities;");
        sb.AppendLine("using TemporalDDD.Infrastructure.WorkflowOrchestration.Activities;");
        sb.AppendLine("using System.Text.Json;");
        sb.AppendLine();
        sb.AppendLine($"[Workflow]");
        sb.AppendLine($"public class {className}");
        sb.AppendLine("{");

        // Generate signal flags for HumanTask nodes
        var humanTaskNodes = workflowDefinition.Nodes.OfType<HumanTaskWorkflowNode>().ToList();
        foreach (var humanTaskNode in humanTaskNodes)
        {
            var signalName = humanTaskNode.SignalName.Value;
            var flagName = $"_signalReceived_{SanitizeIdentifier(signalName)}";
            sb.AppendLine($"    private bool {flagName} = false;");
        }
        sb.AppendLine();

        // Generate signal methods for HumanTask nodes
        foreach (var humanTaskNode in humanTaskNodes)
        {
            var signalName = humanTaskNode.SignalName.Value;
            var flagName = $"_signalReceived_{SanitizeIdentifier(signalName)}";
            var methodName = SanitizeIdentifier(signalName);
            sb.AppendLine($"    [WorkflowSignal]");
            sb.AppendLine($"    public async Task {methodName}Async()");
            sb.AppendLine($"    {{");
            sb.AppendLine($"        {flagName} = true;");
            sb.AppendLine($"    }}");
            sb.AppendLine();
        }

        // Generate the Run method
        sb.AppendLine($"    [WorkflowRun]");
        sb.AppendLine($"    public async Task RunAsync(WorkflowInstancePublicId workflowInstancePublicId)");
        sb.AppendLine($"    {{");
        sb.AppendLine($"        await ExecuteWorkflowAsync(workflowInstancePublicId);");
        sb.AppendLine($"    }}");
        sb.AppendLine();

        // Generate the main execution method with DAG traversal
        sb.AppendLine($"    private async Task ExecuteWorkflowAsync(WorkflowInstancePublicId workflowInstancePublicId)");
        sb.AppendLine($"    {{");
        sb.AppendLine($"        var visited = new HashSet<string>();");
        sb.AppendLine($"        await TraverseFromNodeAsync(\"{startNode.Id.Value}\", visited, nodes, adjacency);");
        sb.AppendLine($"    }}");
        sb.AppendLine();

        // Generate the traversal method
        sb.AppendLine($"    private async Task TraverseFromNodeAsync(string nodeId, HashSet<string> visited, Dictionary<WorkflowNodeId, WorkflowNode> nodes, Dictionary<WorkflowNodeId, List<WorkflowNodeId>> adjacency)");
        sb.AppendLine($"    {{");
        sb.AppendLine($"        if (visited.Contains(nodeId)) return;");
        sb.AppendLine($"        visited.Add(nodeId);");
        sb.AppendLine();
        sb.AppendLine($"        var currentId = WorkflowNodeId.Parse(nodeId).Value;");
        sb.AppendLine($"        var currentNode = nodes[new WorkflowNodeId(currentId)];");
        sb.AppendLine();
        sb.AppendLine($"        switch (currentNode.Type.Value)");
        sb.AppendLine($"        {{");
        sb.AppendLine($"            case 0: // Start");
        sb.AppendLine($"                // Start node - no action");
        sb.AppendLine($"                break;");
        sb.AppendLine($"            case 1: // Api");
        sb.AppendLine($"                {{");
        sb.AppendLine($"                    var apiNode = (ApiWorkflowNode)currentNode;");
        sb.AppendLine($"                    var input = new ExecuteApiInput(");
        sb.AppendLine($"                        nodeId: nodeId,");
        sb.AppendLine($"                        endpointUrl: apiNode.EndpointUrl,");
        sb.AppendLine($"                        authToken: apiNode.AuthToken,");
        sb.AppendLine($"                        mapping: apiNode.ContractMapping);");
        sb.AppendLine($"                    await Workflow.ExecuteActivityAsync(");
        sb.AppendLine($"                        (IWorkflowExecutionActivities act) => act.ExecuteApiCallAsync(input),");
        sb.AppendLine($"                        new ActivityOptions {{ ScheduleToCloseTimeout = TimeSpan.FromMinutes(5) }});");
        sb.AppendLine($"                }}");
        sb.AppendLine($"                break;");
        sb.AppendLine($"            case 2: // Notification");
        sb.AppendLine($"                {{");
        sb.AppendLine($"                    var notificationNode = (NotificationWorkflowNode)currentNode;");
        sb.AppendLine($"                    var input = new SendNotificationInput(");
        sb.AppendLine($"                        nodeId: nodeId,");
        sb.AppendLine($"                        messageTemplate: notificationNode.MessageTemplate);");
        sb.AppendLine($"                    await Workflow.ExecuteActivityAsync(");
        sb.AppendLine($"                        (IWorkflowExecutionActivities act) => act.SendNotificationAsync(input),");
        sb.AppendLine($"                        new ActivityOptions {{ ScheduleToCloseTimeout = TimeSpan.FromMinutes(5) }});");
        sb.AppendLine($"                }}");
        sb.AppendLine($"                break;");
        sb.AppendLine($"            case 3: // HumanTask");
        sb.AppendLine($"                {{");
        sb.AppendLine($"                    var humanTaskNode = (HumanTaskWorkflowNode)currentNode;");
        sb.AppendLine($"                    var signalName = humanTaskNode.SignalName.Value;");
        sb.AppendLine($"                    var sanitizedSignalName = SanitizeIdentifier(signalName);");
        sb.AppendLine($"                    var flagName = $\"_signalReceived_{{sanitizedSignalName}}\";");
        sb.AppendLine($"                    await Workflow.WaitConditionAsync(() => flagName);");
        sb.AppendLine($"                }}");
        sb.AppendLine($"                break;");
        sb.AppendLine($"            case 99: // End");
        sb.AppendLine($"                // End node - no action");
        sb.AppendLine($"                break;");
        sb.AppendLine($"            default:");
        sb.AppendLine($"                throw new InvalidOperationException($\"Unknown node type: {{currentNode.Type}}\");");
        sb.AppendLine($"        }}");
        sb.AppendLine();
        sb.AppendLine($"        // Get outgoing transitions");
        sb.AppendLine($"        if (adjacency.TryGetValue(new WorkflowNodeId(currentId), out var nextNodes))");
        sb.AppendLine($"        {{");
        sb.AppendLine($"            if (nextNodes.Count > 1)");
        sb.AppendLine($"            {{");
        sb.AppendLine($"                // Parallel execution");
        sb.AppendLine($"                var tasks = nextNodes.Select(nextId => TraverseFromNodeAsync(nextId.Value, visited, nodes, adjacency)).ToArray();");
        sb.AppendLine($"                await Task.WhenAll(tasks);");
        sb.AppendLine($"            }}");
        sb.AppendLine($"            else if (nextNodes.Count == 1)");
        sb.AppendLine($"            {{");
        sb.AppendLine($"                // Sequential execution");
        sb.AppendLine($"                await TraverseFromNodeAsync(nextNodes[0].Value, visited, nodes, adjacency);");
        sb.AppendLine($"            }}");
        sb.AppendLine($"        }}");
        sb.AppendLine($"    }}");
        sb.AppendLine("}");

        // Parse the raw string into a syntax tree
        var rawString = sb.ToString();
        var syntaxTree = CSharpSyntaxTree.ParseText(rawString);

        // Check for compilation errors
        var diagnostics = syntaxTree.GetDiagnostics();
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        if (errors.Any())
        {
            var errorMessages = string.Join(Environment.NewLine, errors.Select(e => e.ToString()));
            throw new InvalidOperationException($"Generated C# code has compilation errors:{Environment.NewLine}{errorMessages}");
        }

        // Format the syntax tree
        var root = syntaxTree.GetRoot();
        var formattedRoot = Formatter.Format(root, new AdhocWorkspace());

        // Get the formatted code
        var formattedCode = formattedRoot.ToFullString();

        // Write to file
        var filePath = Path.Combine(outputDirectoryPath, $"{className}.cs");
        await File.WriteAllTextAsync(filePath, formattedCode);

        return formattedCode;
    }

    private static string SanitizeIdentifier(string identifier)
    {
        // Remove invalid characters and ensure it's a valid C# identifier
        var sanitized = new string(identifier.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
        if (string.IsNullOrEmpty(sanitized) || !char.IsLetter(sanitized[0]) && sanitized[0] != '_')
        {
            sanitized = "_" + sanitized;
        }
        return sanitized;
    }
}
