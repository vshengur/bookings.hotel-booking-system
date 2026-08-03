namespace PricingService.Domain.Entities;

/// <summary>
/// Represents the base price for a room type
/// </summary>
public class RoomPrice
{
    public long Id { get; set; }
    public long RoomId { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string Currency { get; set; } = "EUR";
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
