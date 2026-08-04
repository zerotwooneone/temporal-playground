using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.IdentityAndAccess;

public sealed record RoleId
{
    private const string Abbreviation = "ROL";
    public Guid Value { get; }

    private RoleId(Guid value) => Value = value;

    public static Result<RoleId> Create(string value)
    {
        if (string.IsNullOrEmpty(value))
            return Result<RoleId>.Failure("Role ID cannot be null or empty");

        var expectedPrefix = $"{Abbreviation}Id";
        if (!value.StartsWith(expectedPrefix))
            return Result<RoleId>.Failure($"Role ID must start with '{expectedPrefix}'");

        var guidString = value.Substring(expectedPrefix.Length);
        if (!Guid.TryParse(guidString, out var guid))
            return Result<RoleId>.Failure("Invalid GUID format in Role ID");

        return Result<RoleId>.Success(new RoleId(guid));
    }

    public static RoleId New() => new RoleId(Guid.CreateVersion7());

    public override string ToString() => $"{Abbreviation}Id{Value}";
}
