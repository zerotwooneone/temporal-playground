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

        // Generate workflow input record from Start node's output definitions
        var inputRecordName = $"{className}Input";
        var inputRecordFields = new List<string>();
        foreach (var outputDef in startNode.OutputDefinitions)
        {
            var csharpType = MapDataTypeToCSharp(outputDef.DataType);
            var sanitizedName = SanitizeIdentifier(outputDef.PropertyName);
            inputRecordFields.Add($"    {csharpType} {sanitizedName}");
        }

        // Build raw C# code using StringBuilder
        var sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using Temporalio.Workflows;");
        sb.AppendLine("using Temporalio.Activities;");
        sb.AppendLine("using TemporalDDD.Infrastructure.WorkflowOrchestration.Activities;");
        sb.AppendLine("using TemporalDDD.Domain.IdentityAndAccess;");
        sb.AppendLine("using TemporalDDD.Domain.WorkflowOrchestration;");
        sb.AppendLine("using System.Text.Json;");
        sb.AppendLine();

        // Generate workflow input record
        if (inputRecordFields.Any())
        {
            sb.AppendLine($"public record {inputRecordName}(");
            sb.AppendLine(string.Join(",\n", inputRecordFields));
            sb.AppendLine(");");
            sb.AppendLine();
        }

        sb.AppendLine($"[Workflow]");
        sb.AppendLine($"public class {className}");
        sb.AppendLine("{");

        // Generate signal flags for HumanTask nodes
        var humanTaskNodes = workflowDefinition.Nodes.OfType<HumanTaskWorkflowNode>().ToList();
        foreach (var humanTaskNode in humanTaskNodes)
        {
            var signalName = humanTaskNode.SignalName.Value;
            var sanitizedSignalName = SanitizeIdentifier(signalName);
            var flagName = $"_signal_{sanitizedSignalName}";
            sb.AppendLine($"    private bool {flagName} = false;");
        }
        sb.AppendLine();

        // Generate signal methods for HumanTask nodes
        foreach (var humanTaskNode in humanTaskNodes)
        {
            var signalName = humanTaskNode.SignalName.Value;
            var sanitizedSignalName = SanitizeIdentifier(signalName);
            var flagName = $"_signal_{sanitizedSignalName}";
            sb.AppendLine($"    [WorkflowSignal]");
            sb.AppendLine($"    public async Task {sanitizedSignalName}Async() => {flagName} = true;");
        }
        sb.AppendLine();

        // Generate the Run method with unrolled workflow logic
        sb.AppendLine($"    [WorkflowRun]");
        if (inputRecordFields.Any())
        {
            sb.AppendLine($"    public async Task RunAsync({inputRecordName} input)");
        }
        else
        {
            sb.AppendLine($"    public async Task RunAsync()");
        }
        sb.AppendLine($"    {{");

        // Traverse the graph at compile time and generate unrolled code
        var visited = new HashSet<WorkflowNodeId>();
        GenerateNodeExecution(sb, startNode.Id, nodes, adjacency, visited, 1);

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

    private void GenerateNodeExecution(
        StringBuilder sb,
        WorkflowNodeId nodeId,
        Dictionary<WorkflowNodeId, WorkflowNode> nodes,
        Dictionary<WorkflowNodeId, List<WorkflowNodeId>> adjacency,
        HashSet<WorkflowNodeId> visited,
        int indentLevel)
    {
        if (visited.Contains(nodeId))
            return;
        visited.Add(nodeId);

        var currentNode = nodes[nodeId];
        var indent = new string(' ', indentLevel * 4);

        switch (currentNode.Type.Value)
        {
            case 0: // Start
                // Start node - no action
                break;

            case 1: // Api
                {
                    var apiNode = (ApiWorkflowNode)currentNode;
                    var nodeIdStr = nodeId.Value.ToString();
                    var endpointUrl = apiNode.GetTechnicalInput(ApiWorkflowNode.EndpointUrlKey) is InputValueSource.Fixed fixedUrl ? fixedUrl.Value : string.Empty;
                    var authToken = apiNode.GetTechnicalInput(ApiWorkflowNode.AuthTokenKey) is InputValueSource.Fixed fixedToken ? fixedToken.Value : string.Empty;
                    sb.AppendLine($"{indent}var apiInput_{SanitizeIdentifier(nodeIdStr)} = new ExecuteApiInput(");
                    sb.AppendLine($"{indent}    NodeId: \"{nodeIdStr}\",");
                    sb.AppendLine($"{indent}    EndpointUrl: \"{endpointUrl}\",");
                    sb.AppendLine($"{indent}    AuthToken: \"{authToken}\",");
                    sb.AppendLine($"{indent}    Mapping: null);");
                    sb.AppendLine($"{indent}await Workflow.ExecuteActivityAsync(");
                    sb.AppendLine($"{indent}    (IWorkflowExecutionActivities act) => act.ExecuteApiCallAsync(apiInput_{SanitizeIdentifier(nodeIdStr)}),");
                    sb.AppendLine($"{indent}    new ActivityOptions {{ ScheduleToCloseTimeout = TimeSpan.FromMinutes(5) }});");
                }
                break;

            case 2: // Notification
                {
                    var notificationNode = (NotificationWorkflowNode)currentNode;
                    var nodeIdStr = nodeId.Value.ToString();
                    var messageTemplate = notificationNode.GetTechnicalInput(NotificationWorkflowNode.MessageTemplateKey) is InputValueSource.Fixed fixedTemplate ? fixedTemplate.Value : string.Empty;
                    sb.AppendLine($"{indent}var notificationInput_{SanitizeIdentifier(nodeIdStr)} = new SendNotificationInput(");
                    sb.AppendLine($"{indent}    NodeId: \"{nodeIdStr}\",");
                    sb.AppendLine($"{indent}    MessageTemplate: \"{messageTemplate}\");");
                    sb.AppendLine($"{indent}await Workflow.ExecuteActivityAsync(");
                    sb.AppendLine($"{indent}    (IWorkflowExecutionActivities act) => act.SendNotificationAsync(notificationInput_{SanitizeIdentifier(nodeIdStr)}),");
                    sb.AppendLine($"{indent}    new ActivityOptions {{ ScheduleToCloseTimeout = TimeSpan.FromMinutes(5) }});");
                }
                break;

            case 3: // HumanTask
                {
                    var humanTaskNode = (HumanTaskWorkflowNode)currentNode;
                    var signalName = humanTaskNode.SignalName.Value;
                    var sanitizedSignalName = SanitizeIdentifier(signalName);
                    var flagName = $"_signal_{sanitizedSignalName}";
                    sb.AppendLine($"{indent}await Temporalio.Workflows.Workflow.WaitConditionAsync(() => {flagName});");
                }
                break;

            case 99: // End
                // End node - no action
                break;

            default:
                throw new InvalidOperationException($"Unknown node type: {currentNode.Type}");
        }

        // Get outgoing transitions and generate next nodes
        if (adjacency.TryGetValue(nodeId, out var nextNodes))
        {
            if (nextNodes.Count > 1)
            {
                // Parallel execution - generate Task.WhenAll
                sb.AppendLine($"{indent}await Task.WhenAll(");
                for (int i = 0; i < nextNodes.Count; i++)
                {
                    var nextId = nextNodes[i];
                    sb.AppendLine($"{indent}    async () =>");
                    sb.AppendLine($"{indent}    {{");
                    GenerateNodeExecution(sb, nextId, nodes, adjacency, visited, indentLevel + 2);
                    sb.AppendLine($"{indent}    }}");
                    if (i < nextNodes.Count - 1)
                        sb.AppendLine($"{indent},");
                }
                sb.AppendLine($"{indent});");
            }
            else if (nextNodes.Count == 1)
            {
                // Sequential execution
                GenerateNodeExecution(sb, nextNodes[0], nodes, adjacency, visited, indentLevel);
            }
        }
    }

    public static string SanitizeIdentifier(string identifier)
    {
        // Remove invalid characters and ensure it's a valid C# identifier
        var sanitized = new string(identifier.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
        if (string.IsNullOrEmpty(sanitized) || (!char.IsLetter(sanitized[0]) && sanitized[0] != '_'))
        {
            sanitized = "_" + sanitized;
        }
        return sanitized;
    }

    private static string MapDataTypeToCSharp(WorkflowDataType dataType)
    {
        if (dataType is WorkflowDataType.Primitive primitive)
        {
            return primitive.Name switch
            {
                "String" => "string",
                "Number" => "double",
                "Boolean" => "bool",
                "Date" => "DateTime",
                "JsonDocument" => "JsonDocument",
                _ => "object"
            };
        }
        else if (dataType is WorkflowDataType.Semantic semantic)
        {
            // For semantic types, use the underlying primitive type
            return MapDataTypeToCSharp(semantic.UnderlyingType);
        }
        return "object";
    }
}
