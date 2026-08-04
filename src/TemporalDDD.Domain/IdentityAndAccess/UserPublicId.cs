using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.IdentityAndAccess;

public sealed record UserPublicId
{
    private const string Prefix = "USR";
    public Guid Value { get; }

    private UserPublicId(Guid value)
    {
        Value = value;
    }

    public static Result<UserPublicId> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<UserPublicId>.Failure("UserPublicId cannot be null or whitespace");

        var parts = value.Split('_');
        if (parts.Length != 2)
            return Result<UserPublicId>.Failure("UserPublicId must be in format 'PREFIX_Guid'");

        if (parts[0] != Prefix)
            return Result<UserPublicId>.Failure($"UserPublicId must have prefix '{Prefix}'");

        if (!Guid.TryParse(parts[1], out var guidValue))
            return Result<UserPublicId>.Failure("Invalid GUID format in UserPublicId");

        if (guidValue == Guid.Empty)
            return Result<UserPublicId>.Failure("UserPublicId cannot be empty");

        return Result<UserPublicId>.Success(new UserPublicId(guidValue));
    }

    public static UserPublicId New() => new(Guid.NewGuid());

    public static implicit operator Guid(UserPublicId id) => id.Value;

    public override string ToString() => $"{Prefix}_{Value:N}";
}
