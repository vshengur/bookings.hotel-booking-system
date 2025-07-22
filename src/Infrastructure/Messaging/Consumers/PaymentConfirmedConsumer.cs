using System;
using System.Threading.Tasks;
using MassTransit;
using BookingService.Application.Commands;
using BookingService.Application.CommandHandlers;

namespace BookingService.Infrastructure.Messaging.Consumers
{
    public record PaymentConfirmed(Guid BookingId);

    public class PaymentConfirmedConsumer : IConsumer<PaymentConfirmed>
    {
        private readonly ConfirmBookingCommandHandler _handler;

        public PaymentConfirmedConsumer(ConfirmBookingCommandHandler handler) => _handler = handler;

        public async Task Consume(ConsumeContext<PaymentConfirmed> context)
        {
            await _handler.Handle(new ConfirmBookingCommand(context.Message.BookingId));
        }
    }
}
