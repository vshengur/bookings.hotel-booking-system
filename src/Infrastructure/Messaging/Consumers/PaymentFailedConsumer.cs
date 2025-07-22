using System;
using System.Threading.Tasks;
using MassTransit;
using BookingService.Application.Commands;
using BookingService.Application.CommandHandlers;

namespace BookingService.Infrastructure.Messaging.Consumers
{
    public record PaymentFailed(Guid BookingId, string Reason);

    public class PaymentFailedConsumer : IConsumer<PaymentFailed>
    {
        private readonly CancelBookingCommandHandler _handler;

        public PaymentFailedConsumer(CancelBookingCommandHandler handler) => _handler = handler;

        public async Task Consume(ConsumeContext<PaymentFailed> context)
        {
            await _handler.Handle(new CancelBookingCommand(context.Message.BookingId, context.Message.Reason));
        }
    }
}
