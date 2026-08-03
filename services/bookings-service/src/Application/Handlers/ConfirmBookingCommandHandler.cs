using BookingService.Application.Commands;
using BookingService.Domain.Aggregates.Booking;

using MediatR;

using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Application.Handlers;

public class ConfirmBookingCommandHandler : IRequestHandler<ConfirmBookingCommand>
{
    private readonly ISender _sender;

    public ConfirmBookingCommandHandler(ISender sender) => _sender = sender;

    public Task Handle(ConfirmBookingCommand request, CancellationToken cancellationToken) =>
        _sender.Send(
            new SetBookingStatusCommand(request.BookingId, BookingStatus.Confirmed),
            cancellationToken);
}
