using Bookings.Common.Exceptions;

using BookingService.Application.Commands;
using BookingService.Application.Interfaces;
using BookingService.Domain.Aggregates.Booking;

using MediatR;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Application.Handlers;

public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ISender _sender;

    public CancelBookingCommandHandler(IUnitOfWork uow, ISender sender)
    {
        _uow = uow;
        _sender = sender;
    }

    public async Task Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _uow.Bookings.GetAsync(request.BookingId, cancellationToken)
            ?? throw new InvalidOperationException($"Booking {request.BookingId} not found");

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.Confirmed)
            throw new BusinessRuleException("Booking is already finalised");

        var shouldReleaseInventory = booking.Status is BookingStatus.AwaitingPayment or BookingStatus.Reserved;
        var shouldRefundPayment = booking.Status is BookingStatus.Reserved;

        await _sender.Send(
            new SetBookingStatusCommand(request.BookingId, BookingStatus.Cancelled, request.Reason),
            cancellationToken);

        if (shouldReleaseInventory)
            await _sender.Send(new ReleaseInventoryCommand(request.BookingId), cancellationToken);

        if (shouldRefundPayment)
            await _sender.Send(new RefundPaymentCommand(request.BookingId), cancellationToken);
    }
}
