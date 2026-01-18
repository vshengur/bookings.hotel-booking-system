using PricingService.Application.DTOs;

namespace PricingService.Application.Services;

/// <summary>
/// Service interface for pricing rule operations
/// </summary>
public interface IPricingRuleService
{
    Task<PricingRuleDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PricingRuleDto>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<PricingRuleDto>> GetByTypeAsync(string ruleType, CancellationToken cancellationToken = default);
    Task<PricingRuleDto> CreateAsync(CreatePricingRuleRequest request, CancellationToken cancellationToken = default);
    Task<PricingRuleDto?> UpdateAsync(long id, UpdatePricingRuleRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
