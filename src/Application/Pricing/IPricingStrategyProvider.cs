using BookingService.Domain.Patterns;

using System.Threading.Tasks;

namespace BookingService.Application.Pricing;

public interface IPricingStrategyProvider
{
    Task<IPricingStrategy> Resolve(PricingContext ctx);
}
