using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Application.Interfaces;

public interface IUnitOfWork
{
    IBookingRepository Bookings { get; }

    Task SaveChangesAsync(CancellationToken ct = default);
}