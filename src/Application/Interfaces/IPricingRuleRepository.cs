using BookingService.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Application.Pricing;

public interface IPricingRuleRepository
{
    Task<IReadOnlyList<PricingRule>> GetActiveAsync(CancellationToken ct = default);
}