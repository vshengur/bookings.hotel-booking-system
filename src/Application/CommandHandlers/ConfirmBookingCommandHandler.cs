using System;
using System.Threading.Tasks;
using System.Threading;
using BookingService.Application.Commands;
using BookingService.Application.Interfaces;

namespace BookingService.Application.CommandHandlers;

public class ConfirmBookingCommandHandler
{
    private readonly IUnitOfWork _uow;

    public ConfirmBookingCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task Handle(ConfirmBookingCommand command, CancellationToken ct = default)
    {
        var booking = await _uow.Bookings.GetAsync(command.BookingId)
            ?? throw new InvalidOperationException("Booking not found");
        booking.Confirm();
        await _uow.SaveChangesAsync(ct);
    }
}
