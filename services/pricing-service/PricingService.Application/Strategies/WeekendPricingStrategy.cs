using PricingService.Domain.Interfaces;

namespace PricingService.Application.Strategies;

/// <summary>
/// Weekend pricing strategy - applies higher rates for Friday and Saturday nights
/// Weekend nights (Fri, Sat): 1.2x multiplier
/// </summary>
public class WeekendPricingStrategy : IPricingStrategy
{
    public string StrategyName => "Weekend";

    public decimal CalculatePrice(decimal basePrice, DateTime checkIn, DateTime checkOut, PricingContext context)
    {
        decimal totalPrice = 0;
        var currentDate = checkIn.Date;

        // Calculate price for each night
        while (currentDate < checkOut.Date)
        {
            var multiplier = IsWeekendNight(currentDate) ? 1.2m : 1.0m;
            totalPrice += basePrice * multiplier;
            currentDate = currentDate.AddDays(1);
        }

        return totalPrice;
    }

    public bool IsApplicable(PricingContext context)
    {
        // Weekend pricing is always applicable
        return true;
    }

    private bool IsWeekendNight(DateTime date)
    {
        // Friday and Saturday nights are considered weekend
        return date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday;
    }
}
