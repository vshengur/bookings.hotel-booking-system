using System.Threading.Tasks;
using MassTransit;
using BookingService.Application.Commands;
using BookingService.Application.CommandHandlers;
using Bookings.Contracts;

namespace BookingService.Infrastructure.Messaging.Consumers
{
    public class PaymentFailedConsumer : IConsumer<PaymentFailed>
    {
        private readonly CancelBookingCommandHandler _handler;

        public PaymentFailedConsumer(CancelBookingCommandHandler handler) => _handler = handler;

        public async Task Consume(ConsumeContext<PaymentFailed> context)
        {
            await _handler.Handle(new CancelBookingCommand(context.Message.BookingId, context.Message.Error));
        }
    }
}
