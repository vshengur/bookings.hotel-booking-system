using Bookings.Common;
using Bookings.Common.Events;

using MassTransit;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Infrastructure.Messaging;

public sealed class MassTransitDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IPublishEndpoint _publish;

    public MassTransitDomainEventDispatcher(IPublishEndpoint publish) => _publish = publish;

    public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct)
    {
        foreach (var @event in events)
            await _publish.Publish(@event, ct);
    }
}