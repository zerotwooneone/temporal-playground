using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using TemporalDDD.Application.WorkflowOrchestration;
using TemporalDDD.Domain.WorkflowOrchestration;
using TemporalDDD.Domain.WorkflowOrchestration.Nodes;

namespace TemporalDDD.Infrastructure.Generators;

public class RoslynWorkflowCodeGeneratorService : IWorkflowCodeGeneratorService
{
    public async Task<string> GenerateWorkflowClassAsync(WorkflowDefinition workflowDefinition, string outputDirectoryPath)
    {
        var className = workflowDefinition.ClassName.Value;
        var nodes = workflowDefinition.Nodes.ToList();
        var transitions = workflowDefinition.Transitions.ToList();

        // Build adjacency list for graph traversal
        var adjacency = transitions
            .GroupBy(t => t.SourceNodeId)
            .ToDictionary(g => g.Key, g => g.Select(t => t.TargetNodeId).ToList());

        // Find start node
        var startNode = nodes.FirstOrDefault(n => n.Type == NodeType.Start);
        if (startNode == null)
            throw new InvalidOperationException("Workflow must have a Start node");

        // Build raw C# code using StringBuilder
        var sb = new StringBuilder();
        sb.AppendLine("using Temporalio.Workflows;");
        sb.AppendLine("using Temporalio.Activities;");
        sb.AppendLine();
        sb.AppendLine($"[Workflow]");
        sb.AppendLine($"public class {className}");
        sb.AppendLine("{");
        sb.AppendLine($"    [WorkflowRun]");
        sb.AppendLine($"    public async Task RunAsync(WorkflowInstancePublicId workflowInstancePublicId)");
        sb.AppendLine($"    {{");
        sb.AppendLine($"        // Workflow execution logic");
        sb.AppendLine($"        await ExecuteWorkflowAsync(workflowInstancePublicId);");
        sb.AppendLine($"    }}");
        sb.AppendLine();
        sb.AppendLine($"    private async Task ExecuteWorkflowAsync(WorkflowInstancePublicId workflowInstancePublicId)");
        sb.AppendLine($"    {{");
        sb.AppendLine($"        // Traverse the DAG and execute nodes");
        sb.AppendLine($"        await TraverseFromStartAsync(workflowInstancePublicId);");
        sb.AppendLine($"    }}");
        sb.AppendLine();
        sb.AppendLine($"    private async Task TraverseFromStartAsync(WorkflowInstancePublicId workflowInstancePublicId)");
        sb.AppendLine($"    {{");
        sb.AppendLine($"        // Start from the Start node and execute the workflow");
        sb.AppendLine($"        // This is a simplified implementation - actual DAG traversal would be more complex");
        sb.AppendLine($"        await Task.CompletedTask;");
        sb.AppendLine($"    }}");
        sb.AppendLine("}");

        // Parse the raw string into a syntax tree
        var syntaxTree = CSharpSyntaxTree.ParseText(sb.ToString());

        // Get the root of the syntax tree
        var root = syntaxTree.GetRoot();

        // Format the syntax tree
        var formattedRoot = Formatter.Format(root, new AdhocWorkspace());

        // Get the formatted code
        var formattedCode = formattedRoot.ToFullString();

        // Write to file
        var filePath = Path.Combine(outputDirectoryPath, $"{className}.cs");
        await File.WriteAllTextAsync(filePath, formattedCode);

        return formattedCode;
    }
}
