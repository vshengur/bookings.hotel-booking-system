using System;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Application.Abstractions;

public interface IInventoryGateway
{
    Task CheckAvailabilityAsync(long roomId, DateOnly checkIn, DateOnly checkOut, CancellationToken ct);
    Task ReserveAsync(Guid bookingId, long roomId, DateOnly checkIn, DateOnly checkOut, CancellationToken ct);
    Task ReleaseAsync(Guid bookingId, long roomId, CancellationToken ct);
}
