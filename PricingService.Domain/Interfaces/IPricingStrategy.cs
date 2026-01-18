namespace PricingService.Domain.Interfaces;

/// <summary>
/// Strategy pattern interface for pricing calculations
/// </summary>
public interface IPricingStrategy
{
    string StrategyName { get; }
    decimal CalculatePrice(decimal basePrice, DateTime checkIn, DateTime checkOut, PricingContext context);
    bool IsApplicable(PricingContext context);
}

/// <summary>
/// Context for pricing calculations
/// </summary>
public class PricingContext
{
    public long RoomId { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public int Nights { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public int? Occupancy { get; set; } // Current hotel occupancy percentage
    public string? PromoCode { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}
