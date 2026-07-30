using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.SharedKernel;

public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Result<Money> Create(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            return Result<Money>.Failure("Money amount cannot be negative");

        if (string.IsNullOrWhiteSpace(currency))
            return Result<Money>.Failure("Currency cannot be null or whitespace");

        if (currency.Length != 3)
            return Result<Money>.Failure("Currency must be a 3-letter ISO code");

        return Result<Money>.Success(new Money(amount, currency.ToUpperInvariant()));
    }

    public static Money Zero(string currency = "USD") => new(0, currency.ToUpperInvariant());

    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot add money with different currencies: {left.Currency} and {right.Currency}");

        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot subtract money with different currencies: {left.Currency} and {right.Currency}");

        var result = left.Amount - right.Amount;
        if (result < 0)
            throw new InvalidOperationException("Subtraction would result in negative amount");

        return new Money(result, left.Currency);
    }

    public override string ToString() => $"{Amount:C2} {Currency}";
}
