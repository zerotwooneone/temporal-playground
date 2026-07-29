using System.Text.RegularExpressions;

namespace TemporalDDD.Domain.TimesheetProcessing.ValueObjects;

public sealed record PaymentReference
{
    private static readonly Regex PaymentReferenceRegex = new(@"^[A-Z0-9\-]+$", RegexOptions.Compiled);

    public string Value { get; }

    private PaymentReference(string value)
    {
        Value = value;
    }

    public static PaymentReference Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Payment reference cannot be null or whitespace", nameof(value));

        var trimmed = value.Trim().ToUpperInvariant();

        if (trimmed.Length < 4 || trimmed.Length > 100)
            throw new ArgumentException("Payment reference must be between 4 and 100 characters", nameof(value));

        if (!PaymentReferenceRegex.IsMatch(trimmed))
            throw new ArgumentException("Payment reference must contain only alphanumeric characters and hyphens", nameof(value));

        return new PaymentReference(trimmed);
    }

    public static implicit operator string(PaymentReference paymentReference) => paymentReference.Value;

    public override string ToString() => Value;
}
