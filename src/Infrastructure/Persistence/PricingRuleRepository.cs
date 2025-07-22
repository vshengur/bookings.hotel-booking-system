using BookingService.Application.Pricing;
using BookingService.Domain.Entities;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Infrastructure.Persistence;

public sealed class PricingRuleRepository : IPricingRuleRepository
{
    private readonly BookingDbContext _db;

    public PricingRuleRepository(BookingDbContext db) => _db = db;

    public async Task<IReadOnlyList<PricingRule>> GetActiveAsync(CancellationToken ct = default)
        => await _db.PricingRules
                    .Where(r => r.IsActive)
                    .OrderByDescending(r => r.Priority)
                    .ToListAsync(ct);
}