namespace PricingService.Application.DTOs;

/// <summary>
/// Response DTO for price calculation
/// </summary>
public class PriceCalculationResponse
{
    public long RoomId { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = "EUR";
    public int Nights { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public List<AppliedRuleDto> AppliedRules { get; set; } = new();
    public DateTime CalculatedAt { get; set; }
}

public class AppliedRuleDto
{
    public string RuleName { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public decimal PriceImpact { get; set; }
    public decimal Multiplier { get; set; }
}
