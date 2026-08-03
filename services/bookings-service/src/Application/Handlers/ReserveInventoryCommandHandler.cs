using BookingService.Application.Abstractions;
using BookingService.Application.Commands;
using BookingService.Application.Interfaces;

using MediatR;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Application.Handlers;

public class ReserveInventoryCommandHandler : IRequestHandler<ReserveInventoryCommand>
{
    private readonly IInventoryGateway _inv;
    private readonly IUnitOfWork _uow;

    public ReserveInventoryCommandHandler(
        IInventoryGateway inv,
        IUnitOfWork uow)
    {
        _inv = inv;
        _uow = uow;
    }

    public async Task Handle(ReserveInventoryCommand r, CancellationToken ct)
    {
        var booking = await _uow.Bookings.GetAsync(r.BookingId, ct)
            ?? throw new InvalidOperationException($"Booking {r.BookingId} not found");

        foreach (var roomId in booking.Items.Select(i => i.RoomId).Distinct())
            await _inv.ReserveAsync(booking.Id, roomId, booking.CheckInDate, booking.CheckOutDate, ct);
    }
}