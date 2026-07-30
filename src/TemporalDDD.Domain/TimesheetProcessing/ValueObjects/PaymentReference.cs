using System.Text.RegularExpressions;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.TimesheetProcessing.ValueObjects;

public sealed record PaymentReference
{
    private static readonly Regex PaymentReferenceRegex = new(@"^[A-Z0-9\-]+$", RegexOptions.Compiled);

    public string Value { get; }

    private PaymentReference(string value)
    {
        Value = value;
    }

    public static Result<PaymentReference> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<PaymentReference>.Failure("Payment reference cannot be null or whitespace");

        var trimmed = value.Trim().ToUpperInvariant();

        if (trimmed.Length < 4 || trimmed.Length > 100)
            return Result<PaymentReference>.Failure("Payment reference must be between 4 and 100 characters");

        if (!PaymentReferenceRegex.IsMatch(trimmed))
            return Result<PaymentReference>.Failure("Payment reference must contain only alphanumeric characters and hyphens");

        return Result<PaymentReference>.Success(new PaymentReference(trimmed));
    }

    public static implicit operator string(PaymentReference paymentReference) => paymentReference.Value;

    public override string ToString() => Value;
}
