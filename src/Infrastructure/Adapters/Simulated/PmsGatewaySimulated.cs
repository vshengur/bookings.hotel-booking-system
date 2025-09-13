using Bookings.Contracts;

using BookingService.Application.Abstractions;

using MassTransit;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Infrastructure.Adapters.Simulated;


public class PmsGatewaySimulated : IPmsGateway
{
    private readonly IPublishEndpoint _publish;
    public PmsGatewaySimulated(IPublishEndpoint publish) => _publish = publish;

    public Task RequestConfirmationAsync(Guid bookingId, CancellationToken ct)
        => _publish.Publish(new PmsConfirmed(bookingId, $"PMS-{Random.Shared.Next(1000, 9999)}"), ct);
}