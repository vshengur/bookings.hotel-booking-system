using BookingService.Domain.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingService.Domain.Events;

public record PricingRuleCreatedDomainEvent(Guid RuleId, string StrategyKey) : IDomainEvent;
public record PricingRuleChangedDomainEvent(Guid RuleId) : IDomainEvent;
public record PricingRuleActivatedDomainEvent(Guid RuleId) : IDomainEvent;
public record PricingRuleDeactivatedDomainEvent(Guid RuleId) : IDomainEvent;