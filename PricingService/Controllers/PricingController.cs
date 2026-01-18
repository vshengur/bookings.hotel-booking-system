using Microsoft.AspNetCore.Mvc;
using PricingService.Application.DTOs;
using PricingService.Application.Services;

namespace PricingService.Controllers;

/// <summary>
/// REST API controller for pricing operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PricingController : ControllerBase
{
    private readonly IPricingService _pricingService;
    private readonly ILogger<PricingController> _logger;

    public PricingController(IPricingService pricingService, ILogger<PricingController> logger)
    {
        _pricingService = pricingService;
        _logger = logger;
    }

    /// <summary>
    /// Calculate price for a room booking
    /// </summary>
    [HttpPost("calculate")]
    [ProducesResponseType(typeof(PriceCalculationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PriceCalculationResponse>> CalculatePrice([FromBody] PriceCalculationRequest request)
    {
        try
        {
            var response = await _pricingService.CalculatePriceAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while calculating price");
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument while calculating price");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get room price by room ID
    /// </summary>
    [HttpGet("room/{roomId}")]
    [ProducesResponseType(typeof(RoomPriceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoomPriceDto>> GetRoomPrice(long roomId)
    {
        var result = await _pricingService.GetRoomPriceAsync(roomId);
        if (result == null)
        {
            return NotFound(new { error = $"Room price not found for RoomId: {roomId}" });
        }
        return Ok(result);
    }

    /// <summary>
    /// Get all active room prices
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<RoomPriceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RoomPriceDto>>> GetAllActivePrices()
    {
        var result = await _pricingService.GetAllActivePricesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Create a new room price
    /// </summary>
    [HttpPost("room-price")]
    [ProducesResponseType(typeof(RoomPriceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RoomPriceDto>> CreateRoomPrice([FromBody] CreateRoomPriceRequest request)
    {
        try
        {
            var result = await _pricingService.CreateRoomPriceAsync(request);
            return CreatedAtAction(nameof(GetRoomPrice), new { roomId = result.RoomId }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating room price");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update a room price
    /// </summary>
    [HttpPut("room-price/{id}")]
    [ProducesResponseType(typeof(RoomPriceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoomPriceDto>> UpdateRoomPrice(long id, [FromBody] UpdateRoomPriceRequest request)
    {
        var result = await _pricingService.UpdateRoomPriceAsync(id, request);
        if (result == null)
        {
            return NotFound(new { error = $"Room price not found with Id: {id}" });
        }
        return Ok(result);
    }

    /// <summary>
    /// Delete a room price
    /// </summary>
    [HttpDelete("room-price/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteRoomPrice(long id)
    {
        var result = await _pricingService.DeleteRoomPriceAsync(id);
        if (!result)
        {
            return NotFound(new { error = $"Room price not found with Id: {id}" });
        }
        return NoContent();
    }
}
