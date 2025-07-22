using System;
using System.Threading.Tasks;
using BookingService.Application.Commands;
using BookingService.Application.CommandHandlers;

namespace BookingService.Infrastructure.Jobs
{
    public class CancelUnpaidBookingJob
    {
        private readonly CancelBookingCommandHandler _handler;

        public CancelUnpaidBookingJob(CancelBookingCommandHandler handler)
        {
            _handler = handler;
        }

        public Task Execute(Guid bookingId, string reason) => _handler.Handle(new CancelBookingCommand(bookingId, reason));
    }
}
