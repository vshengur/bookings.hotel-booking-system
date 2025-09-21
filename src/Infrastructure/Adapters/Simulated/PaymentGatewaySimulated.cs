using Bookings.Contracts;

using BookingService.Application.Abstractions;

using MassTransit;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Infrastructure.Adapters.Simulated;

public class PaymentGatewaySimulated : IPaymentGateway
{
    private readonly IBus _bus;

    public PaymentGatewaySimulated(IBus bus) => _bus = bus;

    public Task AuthorizeAsync(Guid bookingId, decimal amount, CancellationToken ct)
    {
        _ = Task.Run(async () =>
        {
            // Simulate activity on payment side.
            await Task.Delay(20000);
            _ = _bus.Publish(new PaymentAuthorized(bookingId, $"SIM-{Guid.NewGuid():N}"), ct);
        }, ct);

        return Task.CompletedTask;
        
    }

    public Task RefundAsync(Guid bookingId, CancellationToken ct)
        => Task.CompletedTask; // симулируем успешный refund
}