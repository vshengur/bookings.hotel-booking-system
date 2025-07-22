using System;
using System.Threading;
using System.Threading.Tasks;
using BookingService.Application.Commands;
using BookingService.Application.Interfaces;

namespace BookingService.Application.CommandHandlers
{
    public class CancelBookingCommandHandler
    {
        private readonly IBookingRepository _repo;

        public CancelBookingCommandHandler(IBookingRepository repo) => _repo = repo;

        public async Task Handle(CancelBookingCommand command, CancellationToken ct = default)
        {
            var booking = await _repo.GetAsync(command.BookingId) ?? throw new InvalidOperationException("Booking not found");
            booking.Cancel(command.Reason);
            await _repo.SaveChangesAsync();
        }
    }
}
