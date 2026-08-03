using System;

namespace BookingService.Application;

public static class BookingConstants
{
    /// <summary>
    /// How long a booking stays in AwaitingPayment before the saga cancels it.
    /// Shared by the state machine and the DTO so the frontend receives the exact deadline.
    /// </summary>
    public static readonly TimeSpan PaymentTimeout = TimeSpan.FromMinutes(15);
}
