using System;
using System.Threading.Tasks;
using System.Threading;
using BookingService.Application.Commands;
using BookingService.Application.Interfaces;
using BookingService.Domain.Entities;

namespace BookingService.Application.CommandHandlers
{
    public class ConfirmBookingCommandHandler
    {
        private readonly IBookingRepository _repo;

        public ConfirmBookingCommandHandler(IBookingRepository repo) => _repo = repo;

        public async Task Handle(ConfirmBookingCommand command, CancellationToken ct = default)
        {
            var booking = await _repo.GetAsync(command.BookingId) ?? throw new InvalidOperationException("Booking not found");
            booking.Confirm();
            await _repo.SaveChangesAsync();
        }
    }
}
