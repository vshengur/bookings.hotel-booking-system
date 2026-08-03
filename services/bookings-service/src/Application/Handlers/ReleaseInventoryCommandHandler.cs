using BookingService.Application.Abstractions;
using BookingService.Application.Commands;
using BookingService.Application.Interfaces;

using MediatR;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Application.Handlers;

public class ReleaseInventoryCommandHandler : IRequestHandler<ReleaseInventoryCommand>
{
    private readonly IInventoryGateway _inventoryGateway;
    private readonly IUnitOfWork _uow;

    public ReleaseInventoryCommandHandler(
        IInventoryGateway inventoryGateway,
        IUnitOfWork uow)
    {
        _inventoryGateway = inventoryGateway;
        _uow = uow;
    }

    public async Task Handle(ReleaseInventoryCommand request, CancellationToken cancellationToken)
    {
        var booking = await _uow.Bookings.GetAsync(request.BookingId, cancellationToken)
            ?? throw new InvalidOperationException($"Booking {request.BookingId} not found");

        foreach (var roomId in booking.Items.Select(i => i.RoomId).Distinct())
            await _inventoryGateway.ReleaseAsync(request.BookingId, roomId, cancellationToken);
    }
}
