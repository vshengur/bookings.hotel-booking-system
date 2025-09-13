using BookingService.Application.Commands;
using BookingService.Application.Interfaces;
using BookingService.Domain.Aggregates.Booking;

using MediatR;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Application.Handlers;

public class SetBookingStatusCommandHandler : IRequestHandler<SetBookingStatusCommand>
{
    private readonly IUnitOfWork _uow;

    public SetBookingStatusCommandHandler(IUnitOfWork uow) =>
        _uow = uow;

    public async Task Handle(SetBookingStatusCommand r, CancellationToken ct)
    {

        var booking = await _uow.Bookings.GetAsync(r.BookingId, ct)
            ?? throw new InvalidOperationException($"Booking {r.BookingId} not found");
        
        switch (r.Status)
        {
            case BookingStatus.AwaitingPayment: booking.MarkAwaitingPayment(); break;
            case BookingStatus.Reserved: booking.MarkReserved(); break;
            case BookingStatus.Confirmed: booking.MarkConfirmed(); break;
            case BookingStatus.Cancelled: booking.MarkCancelled(r.CancelReason); break;
            case BookingStatus.Failed: booking.MarkFailed(); break;
            case BookingStatus.Expired: booking.MarkExpired(); break;
        }
        await _uow.Bookings.UpsertAsync(booking, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
