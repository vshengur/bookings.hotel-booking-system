using PricingService.Domain.Interfaces;

namespace PricingService.Application.Strategies;

/// <summary>
/// Promotional pricing strategy - applies discounts based on promo codes or long stays
/// </summary>
public class PromotionalPricingStrategy : IPricingStrategy
{
    private readonly Dictionary<string, decimal> _promoCodes = new()
    {
        { "SUMMER2026", 0.15m },      // 15% discount
        { "EARLYBIRD", 0.20m },       // 20% discount
        { "LASTMINUTE", 0.10m },      // 10% discount
        { "LOYALTY", 0.25m }          // 25% discount for loyal customers
    };

    public string StrategyName => "Promotional";

    public decimal CalculatePrice(decimal basePrice, DateTime checkIn, DateTime checkOut, PricingContext context)
    {
        var totalPrice = basePrice * context.Nights;
        var discount = 0m;

        // Apply promo code discount
        if (!string.IsNullOrEmpty(context.PromoCode) && _promoCodes.TryGetValue(context.PromoCode.ToUpper(), out var promoDiscount))
        {
            discount = promoDiscount;
        }

        // Apply long stay discount (7+ nights: 10%, 14+ nights: 15%, 30+ nights: 20%)
        var longStayDiscount = context.Nights switch
        {
            >= 30 => 0.20m,
            >= 14 => 0.15m,
            >= 7 => 0.10m,
            _ => 0m
        };

        // Use the higher discount
        discount = Math.Max(discount, longStayDiscount);

        return totalPrice * (1 - discount);
    }

    public bool IsApplicable(PricingContext context)
    {
        // Promotional pricing is applicable if there's a promo code or long stay
        return !string.IsNullOrEmpty(context.PromoCode) || context.Nights >= 7;
    }
}
