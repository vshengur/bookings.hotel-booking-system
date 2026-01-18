using PricingService.Domain.Interfaces;

namespace PricingService.Application.Strategies;

/// <summary>
/// Seasonal pricing strategy - applies seasonal multipliers based on dates
/// Peak season: June-August (1.5x)
/// High season: April-May, September-October (1.3x)
/// Low season: November-March (0.9x)
/// </summary>
public class SeasonalPricingStrategy : IPricingStrategy
{
    public string StrategyName => "Seasonal";

    public decimal CalculatePrice(decimal basePrice, DateTime checkIn, DateTime checkOut, PricingContext context)
    {
        decimal totalPrice = 0;
        var currentDate = checkIn.Date;

        // Calculate price for each night with appropriate seasonal multiplier
        while (currentDate < checkOut.Date)
        {
            var multiplier = GetSeasonalMultiplier(currentDate);
            totalPrice += basePrice * multiplier;
            currentDate = currentDate.AddDays(1);
        }

        return totalPrice;
    }

    public bool IsApplicable(PricingContext context)
    {
        // Seasonal pricing is applicable for bookings with any dates
        return true;
    }

    private decimal GetSeasonalMultiplier(DateTime date)
    {
        return date.Month switch
        {
            6 or 7 or 8 => 1.5m,  // Peak season (Summer)
            4 or 5 or 9 or 10 => 1.3m,  // High season (Spring/Fall)
            _ => 0.9m  // Low season (Winter)
        };
    }
}
