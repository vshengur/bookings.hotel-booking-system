using BookingService.Domain.Patterns;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookingService.Application.Pricing;

public class PricingStrategyProvider : IPricingStrategyProvider
{
    private readonly IServiceProvider _sp;
    private readonly IPricingRuleRepository _repo;
    private readonly IMemoryCache _cache;
    private readonly Func<int, OccupancyPricingStrategy> _occupancyFactory;
    private readonly TimeSpan _ttl = TimeSpan.FromMinutes(5);

    public PricingStrategyProvider(IServiceProvider sp,
                                   IPricingRuleRepository repo,
                                   IMemoryCache cache,
                                   Func<int, OccupancyPricingStrategy> occupancyFactory)
    {
        _sp = sp;
        _repo = repo;
        _cache = cache;
        _occupancyFactory = occupancyFactory;
    }

    public async Task<IPricingStrategy> Resolve(PricingContext ctx)
    {
        var rules = await _cache.GetOrCreate("pricing_rules", async e =>
        {
            e.AbsoluteExpirationRelativeToNow = _ttl;
            return await _repo.GetActiveAsync();
        });

        var rule = rules.FirstOrDefault(r =>
           (r.ValidFrom == null || ctx.CheckIn >= r.ValidFrom) &&
           (r.ValidTo == null || ctx.CheckIn <= r.ValidTo));

        var strategyName = rule?.StrategyKey ?? "Standard";

        return strategyName switch
        {
            "Seasonal" => _sp.GetRequiredService<SeasonalPricingStrategy>(),
            "Occupancy" => _occupancyFactory((int)ctx.OccupancyPercent),
            _ => _sp.GetRequiredService<StandardPricingStrategy>()
        };
    }
}

