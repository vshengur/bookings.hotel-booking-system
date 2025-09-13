using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Application.Abstractions;

public interface IPaymentGateway
{
    Task AuthorizeAsync(Guid bookingId, decimal amount, CancellationToken ct);
    Task RefundAsync(Guid bookingId, CancellationToken ct);
}
