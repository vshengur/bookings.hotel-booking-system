using Bookings.Common;

using System;

namespace BookingService.Domain.Events;

public record PricingRuleCreatedDomainEvent(Guid RuleId, string StrategyKey) : IDomainEvent;
public record PricingRuleChangedDomainEvent(Guid RuleId) : IDomainEvent;
public record PricingRuleActivatedDomainEvent(Guid RuleId) : IDomainEvent;
public record PricingRuleDeactivatedDomainEvent(Guid RuleId) : IDomainEvent;