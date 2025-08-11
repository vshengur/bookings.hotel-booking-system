using BookingService.Domain.Entities;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Application.Pricing;

public interface IPricingRuleRepository
{
    Task<IReadOnlyList<PricingRule>> GetActiveAsync(CancellationToken ct = default);
}