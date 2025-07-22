using BookingService.Application.Interfaces;
using BookingService.Application.Pricing;
using BookingService.Infrastructure.Persistence;

using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Infrastructure.UoW;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly BookingDbContext _db;
    public EfUnitOfWork(BookingDbContext db,
                        IBookingRepository bookings,
                        IPricingRuleRepository pricingRules)
    {
        _db = db;
        Bookings = bookings;
        PricingRules = pricingRules;
    }

    public IBookingRepository Bookings { get; }
    public IPricingRuleRepository PricingRules { get; }

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}