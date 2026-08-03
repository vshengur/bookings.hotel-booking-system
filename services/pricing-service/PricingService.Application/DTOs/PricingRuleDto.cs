namespace PricingService.Application.DTOs;

/// <summary>
/// DTO for PricingRule entity
/// </summary>
public class PricingRuleDto
{
    public long Id { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public decimal Multiplier { get; set; }
    public decimal? DiscountPercent { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public int? MinNights { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; }
    public string? Conditions { get; set; }
}

public class CreatePricingRuleRequest
{
    public string RuleName { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public decimal Multiplier { get; set; } = 1.0m;
    public decimal? DiscountPercent { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public int? MinNights { get; set; }
    public int Priority { get; set; } = 0;
    public string? Conditions { get; set; }
}

public class UpdatePricingRuleRequest
{
    public string RuleName { get; set; } = string.Empty;
    public decimal Multiplier { get; set; }
    public decimal? DiscountPercent { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public int? MinNights { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; }
    public string? Conditions { get; set; }
}
