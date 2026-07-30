using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TemporalDDD.Infrastructure.Persistence;

public static class ValueConverters
{
    public static ValueConverter<DateTimeOffset, long> DateTimeOffsetToUnixMillisecondsConverter =>
        new ValueConverter<DateTimeOffset, long>(
            convertToProviderExpression: dto => dto.ToUnixTimeMilliseconds(),
            convertFromProviderExpression: milliseconds => DateTimeOffset.FromUnixTimeMilliseconds(milliseconds));

    public static ValueConverter<DateTime, long> DateTimeToUnixMillisecondsConverter =>
        new ValueConverter<DateTime, long>(
            convertToProviderExpression: dt => new DateTimeOffset(dt).ToUnixTimeMilliseconds(),
            convertFromProviderExpression: milliseconds => DateTime.SpecifyKind(DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).DateTime, DateTimeKind.Utc));

    public static ValueConverter<bool, int> BoolToIntConverter =>
        new ValueConverter<bool, int>(
            convertToProviderExpression: b => b ? 1 : 0,
            convertFromProviderExpression: i => i == 1);

    public static ValueConverter<Guid, string> GuidToStringConverter =>
        new ValueConverter<Guid, string>(
            convertToProviderExpression: g => g.ToString(),
            convertFromProviderExpression: s => Guid.Parse(s));
}
