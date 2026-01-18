namespace PricingService.Application.DTOs;

/// <summary>
/// DTO for RoomPrice entity
/// </summary>
public class RoomPriceDto
{
    public long Id { get; set; }
    public long RoomId { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string Currency { get; set; } = "EUR";
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }
}

public class CreateRoomPriceRequest
{
    public long RoomId { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string Currency { get; set; } = "EUR";
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}

public class UpdateRoomPriceRequest
{
    public decimal BasePrice { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }
}
