using System.Threading.Tasks;
using MassTransit;
using BookingService.Application.Commands;
using BookingService.Application.CommandHandlers;
using Bookings.Contracts;

namespace BookingService.Infrastructure.Messaging.Consumers
{
    public class PaymentConfirmedConsumer : IConsumer<PmsConfirmed>
    {
        private readonly ConfirmBookingCommandHandler _handler;

        public PaymentConfirmedConsumer(ConfirmBookingCommandHandler handler) => _handler = handler;

        public async Task Consume(ConsumeContext<PmsConfirmed> context)
        {
            await _handler.Handle(new ConfirmBookingCommand(context.Message.BookingId));
        }
    }
}
