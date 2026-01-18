using PricingService.Domain.Entities;

namespace PricingService.Domain.Interfaces;

/// <summary>
/// Repository interface for PricingRule entity
/// </summary>
public interface IPricingRuleRepository
{
    Task<PricingRule?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PricingRule>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<PricingRule>> GetActiveByTypeAsync(string ruleType, CancellationToken cancellationToken = default);
    Task<IEnumerable<PricingRule>> GetApplicableRulesAsync(DateTime checkIn, DateTime checkOut, CancellationToken cancellationToken = default);
    Task<PricingRule> AddAsync(PricingRule rule, CancellationToken cancellationToken = default);
    Task UpdateAsync(PricingRule rule, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
