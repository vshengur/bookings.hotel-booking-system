using BookingService.Application.Abstractions;
using BookingService.Application.Commands;

using MediatR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Application.Handlers;

public class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand>
{
    private readonly IPaymentGateway _pay;

    public RefundPaymentCommandHandler(IPaymentGateway pay)
        => _pay = pay;

    public Task Handle(RefundPaymentCommand r, CancellationToken ct)
        => _pay.RefundAsync(r.BookingId, ct);
}