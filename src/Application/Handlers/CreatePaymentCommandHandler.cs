using BookingService.Application.Abstractions;
using BookingService.Application.Commands;
using BookingService.Application.Interfaces;

using MediatR;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Application.Handlers;

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand>
{
    private readonly IPaymentGateway _pay;
    private readonly IUnitOfWork _uow;

    public CreatePaymentCommandHandler(
        IPaymentGateway pay,
        IUnitOfWork uow)
    {
        _pay = pay;
        _uow = uow;
    }

    public async Task Handle(CreatePaymentCommand r, CancellationToken ct)
    {
        var booking = await _uow.Bookings.GetAsync(r.BookingId, ct)
            ?? throw new InvalidOperationException($"Booking {r.BookingId} not found");

        await _pay.AuthorizeAsync(booking.Id, booking.TotalPrice.Amount, ct);
    }
}
