namespace Bookings.Common.ValueObjects;

public sealed record Money(decimal Amount, string Currency = "EUR") : ValueObject
{
    public static Money Zero(string currency = "EUR") => new(0m, currency);

    public static Money operator +(Money a, Money b)
    {
        if (!string.Equals(a.Currency, b.Currency, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Currency mismatch");
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator *(Money money, int nights) =>
        money with { Amount = money.Amount * nights };

    public Money Multiply(int nights)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(nights);
        return new Money(Amount * nights, Currency);
    }
}
