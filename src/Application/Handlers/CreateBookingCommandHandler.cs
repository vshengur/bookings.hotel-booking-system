using BookingService.Application.Commands;
using BookingService.Application.Events;
using BookingService.Application.Interfaces;
using BookingService.Domain.Patterns;

using MediatR;

using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Application.Handlers;

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly IPublisher _publisher;

    public CreateBookingCommandHandler(
        IUnitOfWork uow,
        IPublisher publisher)
    {
        _uow = uow;
        _publisher = publisher;
    }

    async Task IRequestHandler<CreateBookingCommand>.Handle(CreateBookingCommand request, CancellationToken ct)
    {
        var booking = new BookingBuilder()
            .WithId(request.BookingId)
            .ForGuest(request.GuestId)
            .InPeriod(request.CheckIn, request.CheckOut)
            .Build();

        foreach (var i in request.Items)
            booking.AddLineItem(i.RoomId, i.Adults, i.Children, i.Nights, i.PricePerNight);

        await _uow.Bookings.AddAsync(booking, ct);
        await _uow.SaveChangesAsync(ct);

        await _publisher.Publish(new BookingInitiated(request.BookingId), ct);
    }
}
