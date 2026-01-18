namespace PricingService.Domain.Entities;

/// <summary>
/// Represents a pricing rule (seasonal, promotional, etc.)
/// </summary>
public class PricingRule
{
    public long Id { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty; // Seasonal, Weekend, Promotional, Dynamic
    public decimal Multiplier { get; set; } = 1.0m;
    public decimal? DiscountPercent { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public int? MinNights { get; set; }
    public int Priority { get; set; } // Higher priority rules apply first
    public bool IsActive { get; set; } = true;
    public string? Conditions { get; set; } // JSON string with additional conditions
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Rule types constants
/// </summary>
public static class PricingRuleTypes
{
    public const string Seasonal = "Seasonal";
    public const string Weekend = "Weekend";
    public const string Promotional = "Promotional";
    public const string Dynamic = "Dynamic";
    public const string LongStay = "LongStay";
}
