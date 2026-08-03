using PricingService.Domain.Interfaces;

namespace PricingService.Application.Strategies;

/// <summary>
/// Dynamic pricing strategy - adjusts prices based on demand (occupancy rates)
/// High demand (>80% occupancy): 1.4x
/// Medium-high demand (60-80% occupancy): 1.2x
/// Medium demand (40-60% occupancy): 1.0x
/// Low demand (<40% occupancy): 0.85x
/// </summary>
public class DynamicPricingStrategy : IPricingStrategy
{
    public string StrategyName => "Dynamic";

    public decimal CalculatePrice(decimal basePrice, DateTime checkIn, DateTime checkOut, PricingContext context)
    {
        var multiplier = GetDemandMultiplier(context.Occupancy);
        return basePrice * context.Nights * multiplier;
    }

    public bool IsApplicable(PricingContext context)
    {
        // Dynamic pricing is applicable when occupancy data is available
        return context.Occupancy.HasValue;
    }

    private decimal GetDemandMultiplier(int? occupancyPercentage)
    {
        if (!occupancyPercentage.HasValue)
            return 1.0m;

        return occupancyPercentage.Value switch
        {
            > 80 => 1.4m,      // High demand
            > 60 => 1.2m,      // Medium-high demand
            > 40 => 1.0m,      // Medium demand
            _ => 0.85m         // Low demand
        };
    }
}
