using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Application.Abstractions;

public interface IPmsGateway
{
    Task RequestConfirmationAsync(Guid bookingId, CancellationToken ct);
}