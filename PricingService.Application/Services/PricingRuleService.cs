using Microsoft.Extensions.Logging;
using PricingService.Application.DTOs;
using PricingService.Domain.Entities;
using PricingService.Domain.Interfaces;

namespace PricingService.Application.Services;

/// <summary>
/// Service implementation for pricing rule operations
/// </summary>
public class PricingRuleService : IPricingRuleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PricingRuleService> _logger;

    public PricingRuleService(IUnitOfWork unitOfWork, ILogger<PricingRuleService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PricingRuleDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var rule = await _unitOfWork.PricingRules.GetByIdAsync(id, cancellationToken);
        return rule != null ? MapToDto(rule) : null;
    }

    public async Task<IEnumerable<PricingRuleDto>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var rules = await _unitOfWork.PricingRules.GetAllActiveAsync(cancellationToken);
        return rules.Select(MapToDto);
    }

    public async Task<IEnumerable<PricingRuleDto>> GetByTypeAsync(string ruleType, CancellationToken cancellationToken = default)
    {
        var rules = await _unitOfWork.PricingRules.GetActiveByTypeAsync(ruleType, cancellationToken);
        return rules.Select(MapToDto);
    }

    public async Task<PricingRuleDto> CreateAsync(CreatePricingRuleRequest request, CancellationToken cancellationToken = default)
    {
        var rule = new PricingRule
        {
            RuleName = request.RuleName,
            RuleType = request.RuleType,
            Multiplier = request.Multiplier,
            DiscountPercent = request.DiscountPercent,
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo,
            MinNights = request.MinNights,
            Priority = request.Priority,
            IsActive = true,
            Conditions = request.Conditions,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _unitOfWork.PricingRules.AddAsync(rule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created pricing rule: {RuleName}", request.RuleName);

        return MapToDto(created);
    }

    public async Task<PricingRuleDto?> UpdateAsync(long id, UpdatePricingRuleRequest request, CancellationToken cancellationToken = default)
    {
        var rule = await _unitOfWork.PricingRules.GetByIdAsync(id, cancellationToken);
        if (rule == null)
        {
            return null;
        }

        rule.RuleName = request.RuleName;
        rule.Multiplier = request.Multiplier;
        rule.DiscountPercent = request.DiscountPercent;
        rule.ValidFrom = request.ValidFrom;
        rule.ValidTo = request.ValidTo;
        rule.MinNights = request.MinNights;
        rule.Priority = request.Priority;
        rule.IsActive = request.IsActive;
        rule.Conditions = request.Conditions;
        rule.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.PricingRules.UpdateAsync(rule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated pricing rule Id: {Id}", id);

        return MapToDto(rule);
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var rule = await _unitOfWork.PricingRules.GetByIdAsync(id, cancellationToken);
        if (rule == null)
        {
            return false;
        }

        await _unitOfWork.PricingRules.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted pricing rule Id: {Id}", id);

        return true;
    }

    private static PricingRuleDto MapToDto(PricingRule entity)
    {
        return new PricingRuleDto
        {
            Id = entity.Id,
            RuleName = entity.RuleName,
            RuleType = entity.RuleType,
            Multiplier = entity.Multiplier,
            DiscountPercent = entity.DiscountPercent,
            ValidFrom = entity.ValidFrom,
            ValidTo = entity.ValidTo,
            MinNights = entity.MinNights,
            Priority = entity.Priority,
            IsActive = entity.IsActive,
            Conditions = entity.Conditions
        };
    }
}
