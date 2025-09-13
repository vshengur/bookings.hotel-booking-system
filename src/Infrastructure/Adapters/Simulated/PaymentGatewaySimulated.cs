using Bookings.Contracts;

using BookingService.Application.Abstractions;

using MassTransit;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Infrastructure.Adapters.Simulated;

public class PaymentGatewaySimulated : IPaymentGateway
{
    private readonly IPublishEndpoint _publish;

    public PaymentGatewaySimulated(IPublishEndpoint publish) => _publish = publish;

    public Task AuthorizeAsync(Guid bookingId, decimal amount, CancellationToken ct)
        => _publish.Publish(new PaymentAuthorized(bookingId, $"SIM-{Guid.NewGuid():N}"), ct);

    public Task RefundAsync(Guid bookingId, CancellationToken ct)
        => Task.CompletedTask; // симулируем успешный refund
}