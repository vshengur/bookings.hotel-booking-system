using Bookings.Common;

using BookingService.Domain.Events;

using System;

namespace BookingService.Domain.Entities;

public sealed class PricingRule : Entity, IAggregateRoot
{
    public Guid Id { get; private set; }
    public string StrategyKey { get; private set; }
    public DateOnly? ValidFrom { get; private set; }
    public DateOnly? ValidTo { get; private set; }
    public int Priority { get; private set; }
    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private PricingRule() { }

    private PricingRule(Guid id, string strategyKey, int priority)
    {
        Id = id;
        StrategyKey = strategyKey;
        Priority = priority;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;

        AddEvent(new PricingRuleCreatedDomainEvent(id, strategyKey));
    }

    public static PricingRule Create(string strategyKey, int priority = 0) =>
        new(Guid.NewGuid(), strategyKey, priority);

    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
            UpdatedAtUtc = DateTime.UtcNow;
            AddEvent(new PricingRuleActivatedDomainEvent(Id));
        }
    }

    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            UpdatedAtUtc = DateTime.UtcNow;
            AddEvent(new PricingRuleDeactivatedDomainEvent(Id));
        }
    }

    public void Change(
        string? strategyKey = null,
        DateOnly? validFrom = null,
        DateOnly? validTo = null,
        int? minOccupancyPercent = null,
        string? promoCode = null,
        int? priority = null)
    {
        StrategyKey = strategyKey ?? StrategyKey;
        ValidFrom = validFrom ?? ValidFrom;
        ValidTo = validTo ?? ValidTo;
        Priority = priority ?? Priority;

        UpdatedAtUtc = DateTime.UtcNow;
        AddEvent(new PricingRuleChangedDomainEvent(Id));
    }
}