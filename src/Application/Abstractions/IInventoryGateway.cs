using System;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Application.Abstractions;

public interface IInventoryGateway
{
    Task ReserveAsync(Guid bookingId, Guid roomId, DateOnly checkIn, DateOnly checkOut, CancellationToken ct);
    Task ReleaseAsync(Guid bookingId, CancellationToken ct);
}