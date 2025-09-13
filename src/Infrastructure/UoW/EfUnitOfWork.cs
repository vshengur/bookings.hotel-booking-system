using BookingService.Application.Interfaces;
using BookingService.Infrastructure.Persistence;

using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Infrastructure.UoW;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly BookingDbContext _db;
    public EfUnitOfWork(BookingDbContext db,
                        IBookingRepository bookings)
    {
        _db = db;
        Bookings = bookings;
    }

    public IBookingRepository Bookings { get; }

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}