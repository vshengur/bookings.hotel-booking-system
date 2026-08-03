using Bookings.Contracts;

using MassTransit;

using Microsoft.Extensions.Logging;

using PaymentService.Application.Messages;

using System;
using System.Threading.Tasks;

namespace BookingService.Infrastructure.Messaging.MassTransit;

public class PaymentStatusChangedConsumer : IConsumer<PaymentStatusChanged>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PaymentStatusChangedConsumer> _logger;

    public PaymentStatusChangedConsumer(
        IPublishEndpoint publishEndpoint,
        ILogger<PaymentStatusChangedConsumer> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentStatusChanged> context)
    {
        var status = context.Message.Status?.Trim();

        switch (status?.ToLowerInvariant())
        {
            case "succeeded":
                await _publishEndpoint.Publish(
                    new PaymentAuthorized(
                        context.Message.BookingId,
                        $"payment-status:{context.Message.BookingId:N}"),
                    context.CancellationToken);
                break;

            case "failed":
                await _publishEndpoint.Publish(
                    new PaymentFailed(
                        context.Message.BookingId,
                        "Payment provider reported a failed payment."),
                    context.CancellationToken);
                break;

            case "refunded":
                _logger.LogInformation(
                    "Ignoring refunded payment status for booking {BookingId}; booking cancellation flow already owns this transition.",
                    context.Message.BookingId);
                break;

            default:
                _logger.LogWarning(
                    "Ignoring unsupported payment status '{Status}' for booking {BookingId}",
                    status,
                    context.Message.BookingId);
                break;
        }
    }
}
