using System.Text.RegularExpressions;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.WorkflowOrchestration.ValueObjects;

public sealed record WorkflowClassName
{
    public string Value { get; }

    private WorkflowClassName(string value) => Value = value;

    public static Result<WorkflowClassName> Create(string rawName, WorkflowDefinitionPublicId publicId)
    {
        if (string.IsNullOrWhiteSpace(rawName))
            return Result<WorkflowClassName>.Failure("Workflow name cannot be null or empty");

        // Sanitize: Remove all non-alphanumeric characters
        var sanitizedName = Regex.Replace(rawName.Trim(), @"[^a-zA-Z0-9]", "");

        if (string.IsNullOrWhiteSpace(sanitizedName))
            return Result<WorkflowClassName>.Failure("Workflow name must contain at least one alphanumeric character");

        // Ensure valid start: Prepend underscore if starts with a number
        if (char.IsDigit(sanitizedName[0]))
        {
            sanitizedName = $"_{sanitizedName}";
        }

        // Uniqueness: Take first 8 characters of the PublicId Guid (without hyphens)
        var shortId = publicId.Value.ToString("N").Substring(0, 8);

        // Combine: {SanitizedName}_{ShortId}
        var combinedValue = $"{sanitizedName}_{shortId}";

        return Result<WorkflowClassName>.Success(new WorkflowClassName(combinedValue));
    }

    public override string ToString() => Value;
}
