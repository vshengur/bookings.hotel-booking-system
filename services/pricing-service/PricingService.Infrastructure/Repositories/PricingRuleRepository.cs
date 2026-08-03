using Microsoft.EntityFrameworkCore;
using PricingService.Domain.Entities;
using PricingService.Domain.Interfaces;
using PricingService.Infrastructure.Data;

namespace PricingService.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for PricingRule entity
/// </summary>
public class PricingRuleRepository : IPricingRuleRepository
{
    private readonly PricingDbContext _context;

    public PricingRuleRepository(PricingDbContext context)
    {
        _context = context;
    }

    public async Task<PricingRule?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.PricingRules
            .FirstOrDefaultAsync(pr => pr.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<PricingRule>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PricingRules
            .Where(pr => pr.IsActive)
            .OrderByDescending(pr => pr.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PricingRule>> GetActiveByTypeAsync(string ruleType, CancellationToken cancellationToken = default)
    {
        return await _context.PricingRules
            .Where(pr => pr.RuleType == ruleType && pr.IsActive)
            .OrderByDescending(pr => pr.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PricingRule>> GetApplicableRulesAsync(DateTime checkIn, DateTime checkOut, CancellationToken cancellationToken = default)
    {
        return await _context.PricingRules
            .Where(pr => pr.IsActive
                && (pr.ValidFrom == null || pr.ValidFrom <= checkIn)
                && (pr.ValidTo == null || pr.ValidTo >= checkOut))
            .OrderByDescending(pr => pr.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<PricingRule> AddAsync(PricingRule rule, CancellationToken cancellationToken = default)
    {
        await _context.PricingRules.AddAsync(rule, cancellationToken);
        return rule;
    }

    public Task UpdateAsync(PricingRule rule, CancellationToken cancellationToken = default)
    {
        _context.PricingRules.Update(rule);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var rule = await GetByIdAsync(id, cancellationToken);
        if (rule != null)
        {
            _context.PricingRules.Remove(rule);
        }
    }
}
