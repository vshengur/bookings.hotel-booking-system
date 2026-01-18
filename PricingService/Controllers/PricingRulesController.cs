using Microsoft.AspNetCore.Mvc;
using PricingService.Application.DTOs;
using PricingService.Application.Services;

namespace PricingService.Controllers;

/// <summary>
/// REST API controller for pricing rules management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PricingRulesController : ControllerBase
{
    private readonly IPricingRuleService _pricingRuleService;
    private readonly ILogger<PricingRulesController> _logger;

    public PricingRulesController(IPricingRuleService pricingRuleService, ILogger<PricingRulesController> logger)
    {
        _pricingRuleService = pricingRuleService;
        _logger = logger;
    }

    /// <summary>
    /// Get pricing rule by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PricingRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PricingRuleDto>> GetById(long id)
    {
        var result = await _pricingRuleService.GetByIdAsync(id);
        if (result == null)
        {
            return NotFound(new { error = $"Pricing rule not found with Id: {id}" });
        }
        return Ok(result);
    }

    /// <summary>
    /// Get all active pricing rules
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<PricingRuleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PricingRuleDto>>> GetAllActive()
    {
        var result = await _pricingRuleService.GetAllActiveAsync();
        return Ok(result);
    }

    /// <summary>
    /// Get pricing rules by type
    /// </summary>
    [HttpGet("type/{ruleType}")]
    [ProducesResponseType(typeof(IEnumerable<PricingRuleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PricingRuleDto>>> GetByType(string ruleType)
    {
        var result = await _pricingRuleService.GetByTypeAsync(ruleType);
        return Ok(result);
    }

    /// <summary>
    /// Create a new pricing rule
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PricingRuleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PricingRuleDto>> Create([FromBody] CreatePricingRuleRequest request)
    {
        try
        {
            var result = await _pricingRuleService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating pricing rule");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update a pricing rule
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PricingRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PricingRuleDto>> Update(long id, [FromBody] UpdatePricingRuleRequest request)
    {
        var result = await _pricingRuleService.UpdateAsync(id, request);
        if (result == null)
        {
            return NotFound(new { error = $"Pricing rule not found with Id: {id}" });
        }
        return Ok(result);
    }

    /// <summary>
    /// Delete a pricing rule
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(long id)
    {
        var result = await _pricingRuleService.DeleteAsync(id);
        if (!result)
        {
            return NotFound(new { error = $"Pricing rule not found with Id: {id}" });
        }
        return NoContent();
    }
}
