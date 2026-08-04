using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.IdentityAndAccess;

public sealed record Permission
{
    public int Value { get; }
    public string Name { get; }
    public string Code { get; }

    private Permission(int value, string name, string code)
    {
        Value = value;
        Name = name;
        Code = code;
    }

    public static readonly Permission CreateWorkflow = new(10, "Create Workflow", "workflow:create");
    public static readonly Permission EditWorkflow = new(11, "Edit Workflow", "workflow:edit");
    public static readonly Permission ApproveWorkflow = new(12, "Approve Workflow", "workflow:approve");
    public static readonly Permission ConfigureApiNode = new(20, "Configure API Node", "node:api:configure");
    public static readonly Permission ConfigureQueryNode = new(21, "Configure Query Node", "node:query:configure");

    private static readonly Permission[] AllPermissions = { CreateWorkflow, EditWorkflow, ApproveWorkflow, ConfigureApiNode, ConfigureQueryNode };

    public static Result<Permission> FromValue(int value)
    {
        var permission = AllPermissions.FirstOrDefault(p => p.Value == value);
        if (permission == null)
            return Result<Permission>.Failure($"Invalid Permission value: {value}");
        return Result<Permission>.Success(permission);
    }

    public static Result<Permission> FromCode(string code)
    {
        var permission = AllPermissions.FirstOrDefault(p => p.Code == code);
        if (permission == null)
            return Result<Permission>.Failure($"Invalid Permission code: {code}");
        return Result<Permission>.Success(permission);
    }

    public static implicit operator int(Permission permission) => permission.Value;

    public override string ToString() => Name;
}
