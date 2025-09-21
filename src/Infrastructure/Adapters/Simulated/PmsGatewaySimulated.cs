using Bookings.Contracts;

using BookingService.Application.Abstractions;

using MassTransit;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Infrastructure.Adapters.Simulated;


public class PmsGatewaySimulated : IPmsGateway
{
    private readonly IBus _bus;

    public PmsGatewaySimulated(IBus bus) => _bus = bus;

    public Task RequestConfirmationAsync(Guid bookingId, CancellationToken ct)
    {
        _ = Task.Run(async () =>
        {
            // Simulate activity on payment side.
            await Task.Delay(20000);
            _ = _bus.Publish(new PmsConfirmed(bookingId, $"PMS-{Random.Shared.Next(1000, 9999)}"), ct);
        }, ct);

        return Task.CompletedTask;
    }
}