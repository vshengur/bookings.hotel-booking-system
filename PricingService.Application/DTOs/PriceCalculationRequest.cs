namespace PricingService.Application.DTOs;

/// <summary>
/// Request DTO for price calculation
/// </summary>
public class PriceCalculationRequest
{
    public long RoomId { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public int? Occupancy { get; set; }
    public string? PromoCode { get; set; }
}
