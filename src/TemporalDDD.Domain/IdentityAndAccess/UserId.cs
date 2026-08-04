using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.IdentityAndAccess;

public sealed record UserId
{
    private const string Abbreviation = "USR";
    public Guid Value { get; }

    private UserId(Guid value) => Value = value;

    public static Result<UserId> Create(string value)
    {
        if (string.IsNullOrEmpty(value))
            return Result<UserId>.Failure("User ID cannot be null or empty");

        var expectedPrefix = $"{Abbreviation}Id";
        if (!value.StartsWith(expectedPrefix))
            return Result<UserId>.Failure($"User ID must start with '{expectedPrefix}'");

        var guidString = value.Substring(expectedPrefix.Length);
        if (!Guid.TryParse(guidString, out var guid))
            return Result<UserId>.Failure("Invalid GUID format in User ID");

        return Result<UserId>.Success(new UserId(guid));
    }

    public static UserId New() => new UserId(Guid.CreateVersion7());

    public override string ToString() => $"{Abbreviation}Id{Value}";
}
