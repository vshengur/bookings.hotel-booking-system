using BookingService.Application.Pricing;

using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Application.Interfaces;

public interface IUnitOfWork
{
    IBookingRepository Bookings { get; }
    IPricingRuleRepository PricingRules { get; }

    Task SaveChangesAsync(CancellationToken ct = default);
}