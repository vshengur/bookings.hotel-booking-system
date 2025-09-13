using BookingService.Application.Abstractions;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Infrastructure.Adapters.Simulated;

public class InventoryGatewaySimulated : IInventoryGateway
{
    public Task ReserveAsync(Guid bookingId, Guid roomId, DateOnly checkIn, DateOnly checkOut, CancellationToken ct)
        => Task.CompletedTask;

    public Task ReleaseAsync(Guid bookingId, CancellationToken ct)
        => Task.CompletedTask;
}