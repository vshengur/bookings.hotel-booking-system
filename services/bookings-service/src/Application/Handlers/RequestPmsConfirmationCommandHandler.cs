using BookingService.Application.Abstractions;
using BookingService.Application.Commands;

using MediatR;

using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Application.Handlers;

public class RequestPmsConfirmationCommandHandler : IRequestHandler<RequestPmsConfirmationCommand>
{
    private readonly IPmsGateway _pms;

    public RequestPmsConfirmationCommandHandler(IPmsGateway pms)
        => _pms = pms;

    public Task Handle(RequestPmsConfirmationCommand r, CancellationToken ct) =>
        _pms.RequestConfirmationAsync(r.BookingId, ct);
}
