using Microsoft.Extensions.Logging;
using PricingService.Application.DTOs;
using PricingService.Domain.Entities;
using PricingService.Domain.Interfaces;

namespace PricingService.Application.Services;

/// <summary>
/// Service implementation for pricing operations
/// </summary>
public class PricingService : IPricingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnumerable<IPricingStrategy> _strategies;
    private readonly ILogger<PricingService> _logger;

    public PricingService(
        IUnitOfWork unitOfWork,
        IEnumerable<IPricingStrategy> strategies,
        ILogger<PricingService> logger)
    {
        _unitOfWork = unitOfWork;
        _strategies = strategies;
        _logger = logger;
    }

    public async Task<PriceCalculationResponse> CalculatePriceAsync(
        PriceCalculationRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calculating price for RoomId: {RoomId}, CheckIn: {CheckIn}, CheckOut: {CheckOut}",
            request.RoomId, request.CheckIn, request.CheckOut);

        // Get base price
        var roomPrice = await _unitOfWork.RoomPrices.GetByRoomIdAsync(request.RoomId, cancellationToken);
        if (roomPrice == null)
        {
            _logger.LogWarning("Room price not found for RoomId: {RoomId}", request.RoomId);
            throw new InvalidOperationException($"Room price not found for RoomId: {request.RoomId}");
        }

        if (!roomPrice.IsActive)
        {
            _logger.LogWarning("Room price is not active for RoomId: {RoomId}", request.RoomId);
            throw new InvalidOperationException($"Room price is not active for RoomId: {request.RoomId}");
        }

        var nights = (request.CheckOut.Date - request.CheckIn.Date).Days;
        if (nights <= 0)
        {
            throw new ArgumentException("Check-out date must be after check-in date");
        }

        // Create pricing context
        var context = new PricingContext
        {
            RoomId = request.RoomId,
            RoomType = request.RoomType,
            Nights = nights,
            CheckIn = request.CheckIn,
            CheckOut = request.CheckOut,
            Occupancy = request.Occupancy,
            PromoCode = request.PromoCode
        };

        // Apply strategies and calculate final price
        var basePrice = roomPrice.BasePrice;
        var finalPrice = basePrice;
        var appliedRules = new List<AppliedRuleDto>();

        // Select the best applicable strategy
        var applicableStrategy = _strategies
            .Where(s => s.IsApplicable(context))
            .OrderByDescending(s => GetStrategyPriority(s.StrategyName))
            .FirstOrDefault();

        if (applicableStrategy != null)
        {
            var strategyPrice = applicableStrategy.CalculatePrice(basePrice, request.CheckIn, request.CheckOut, context);
            var priceImpact = strategyPrice - (basePrice * nights);

            appliedRules.Add(new AppliedRuleDto
            {
                RuleName = applicableStrategy.StrategyName,
                RuleType = applicableStrategy.StrategyName,
                PriceImpact = priceImpact,
                Multiplier = strategyPrice / (basePrice * nights)
            });

            finalPrice = strategyPrice / nights; // Average price per night
        }

        var totalPrice = finalPrice * nights;

        _logger.LogInformation("Price calculated - BasePrice: {BasePrice}, FinalPrice: {FinalPrice}, TotalPrice: {TotalPrice}",
            basePrice, finalPrice, totalPrice);

        return new PriceCalculationResponse
        {
            RoomId = request.RoomId,
            RoomType = request.RoomType,
            BasePrice = basePrice,
            FinalPrice = finalPrice,
            TotalPrice = totalPrice,
            Currency = roomPrice.Currency,
            Nights = nights,
            CheckIn = request.CheckIn,
            CheckOut = request.CheckOut,
            AppliedRules = appliedRules,
            CalculatedAt = DateTime.UtcNow
        };
    }

    public async Task<RoomPriceDto?> GetRoomPriceAsync(long roomId, CancellationToken cancellationToken = default)
    {
        var roomPrice = await _unitOfWork.RoomPrices.GetByRoomIdAsync(roomId, cancellationToken);
        return roomPrice != null ? MapToDto(roomPrice) : null;
    }

    public async Task<IEnumerable<RoomPriceDto>> GetAllActivePricesAsync(CancellationToken cancellationToken = default)
    {
        var prices = await _unitOfWork.RoomPrices.GetAllActiveAsync(cancellationToken);
        return prices.Select(MapToDto);
    }

    public async Task<RoomPriceDto> CreateRoomPriceAsync(
        CreateRoomPriceRequest request,
        CancellationToken cancellationToken = default)
    {
        var roomPrice = new RoomPrice
        {
            RoomId = request.RoomId,
            RoomType = request.RoomType,
            BasePrice = request.BasePrice,
            Currency = request.Currency,
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _unitOfWork.RoomPrices.AddAsync(roomPrice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created room price for RoomId: {RoomId}", request.RoomId);

        return MapToDto(created);
    }

    public async Task<RoomPriceDto?> UpdateRoomPriceAsync(
        long id,
        UpdateRoomPriceRequest request,
        CancellationToken cancellationToken = default)
    {
        var roomPrice = await _unitOfWork.RoomPrices.GetByIdAsync(id, cancellationToken);
        if (roomPrice == null)
        {
            return null;
        }

        roomPrice.BasePrice = request.BasePrice;
        roomPrice.ValidFrom = request.ValidFrom;
        roomPrice.ValidTo = request.ValidTo;
        roomPrice.IsActive = request.IsActive;
        roomPrice.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.RoomPrices.UpdateAsync(roomPrice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated room price Id: {Id}", id);

        return MapToDto(roomPrice);
    }

    public async Task<bool> DeleteRoomPriceAsync(long id, CancellationToken cancellationToken = default)
    {
        var roomPrice = await _unitOfWork.RoomPrices.GetByIdAsync(id, cancellationToken);
        if (roomPrice == null)
        {
            return false;
        }

        await _unitOfWork.RoomPrices.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted room price Id: {Id}", id);

        return true;
    }

    private static RoomPriceDto MapToDto(RoomPrice entity)
    {
        return new RoomPriceDto
        {
            Id = entity.Id,
            RoomId = entity.RoomId,
            RoomType = entity.RoomType,
            BasePrice = entity.BasePrice,
            Currency = entity.Currency,
            ValidFrom = entity.ValidFrom,
            ValidTo = entity.ValidTo,
            IsActive = entity.IsActive
        };
    }

    private int GetStrategyPriority(string strategyName)
    {
        // Define strategy priority (higher number = higher priority)
        return strategyName switch
        {
            "Promotional" => 5,  // Highest priority
            "Dynamic" => 4,
            "Seasonal" => 3,
            "Weekend" => 2,
            "Base" => 1,  // Lowest priority (fallback)
            _ => 0
        };
    }
}
