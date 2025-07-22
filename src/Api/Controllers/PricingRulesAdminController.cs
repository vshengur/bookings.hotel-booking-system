using BookingService.Domain.Entities;
using BookingService.Infrastructure.Persistence;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingService.Api.Controllers;

[ApiController]
[Route("api/admin/pricing-rules")]
public class PricingRulesAdminController : ControllerBase
{
    private readonly BookingDbContext _db;

    public PricingRulesAdminController(BookingDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PricingRule>>> Get() =>
        Ok(await _db.PricingRules.AsNoTracking().ToListAsync());

    [HttpPost]
    public async Task<ActionResult> Create(PricingRule rule)
    {
        _db.PricingRules.Add(rule);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { rule.Id }, rule);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, PricingRule updated)
    {
        var rule = await _db.PricingRules.FindAsync(id);
        if (rule is null) return NotFound();

        _db.Entry(rule).CurrentValues.SetValues(updated);
        await _db.SaveChangesAsync();

        // сбрасываем кэш
        HttpContext.RequestServices
                   .GetRequiredService<IMemoryCache>()
                   .Remove("pricing_rules");

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var rule = await _db.PricingRules.FindAsync(id);
        if (rule is null) return NotFound();

        _db.PricingRules.Remove(rule);
        await _db.SaveChangesAsync();
        HttpContext.RequestServices
                   .GetRequiredService<IMemoryCache>()
                   .Remove("pricing_rules");
        return NoContent();
    }
}