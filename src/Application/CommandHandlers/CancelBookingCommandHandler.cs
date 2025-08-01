using System;
using System.Threading;
using System.Threading.Tasks;
using BookingService.Application.Commands;
using BookingService.Application.Interfaces;

namespace BookingService.Application.CommandHandlers
{
    public class CancelBookingCommandHandler
    {
        private readonly IUnitOfWork _uow;

        public CancelBookingCommandHandler(IUnitOfWork uow) => _uow = uow;

        public async Task Handle(CancelBookingCommand command, CancellationToken ct = default)
        {
            var booking = await _uow.Bookings.GetAsync(command.BookingId)
                ?? throw new InvalidOperationException("Booking not found");
            booking.Cancel(command.Reason);
            await _uow.SaveChangesAsync(ct);
        }
    }
}
