using PricingService.Domain.Interfaces;

namespace PricingService.Application.Strategies;

/// <summary>
/// Base pricing strategy - returns the base price without modifications
/// </summary>
public class BasePricingStrategy : IPricingStrategy
{
    public string StrategyName => "Base";

    public decimal CalculatePrice(decimal basePrice, DateTime checkIn, DateTime checkOut, PricingContext context)
    {
        return basePrice * context.Nights;
    }

    public bool IsApplicable(PricingContext context)
    {
        // Base strategy is always applicable as a fallback
        return true;
    }
}
