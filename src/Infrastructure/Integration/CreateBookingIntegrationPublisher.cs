using Bookings.Contracts;

using BookingService.Application.Events;

using MassTransit;

using MediatR;

using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Infrastructure.Integration;

/// <summary>
/// Translates internal app event to external bus contract to trigger the saga.
/// Keeps Application free of MassTransit/Contracts.
/// </summary>
public class CreateBookingIntegrationPublisher : INotificationHandler<BookingInitiated>
{
    private readonly IBus _bus;

    public CreateBookingIntegrationPublisher(IBus bus) =>
        _bus = bus;

    public Task Handle(BookingInitiated n, CancellationToken ct)
        => _bus.Publish(new CreateBooking(n.BookingId), ct);
}