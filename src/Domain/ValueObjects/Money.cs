using BookingService.Domain.Common;

namespace BookingService.Domain.ValueObjects;

public sealed record Money(decimal Amount, string Currency = "EUR") : ValueObject
{
    public static Money operator *(Money money, int nights) =>
        money with { Amount = money.Amount * nights };
}