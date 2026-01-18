using PricingService.Application.DTOs;

namespace PricingService.Application.Services;

/// <summary>
/// Service interface for pricing operations
/// </summary>
public interface IPricingService
{
    Task<PriceCalculationResponse> CalculatePriceAsync(PriceCalculationRequest request, CancellationToken cancellationToken = default);
    Task<RoomPriceDto?> GetRoomPriceAsync(long roomId, CancellationToken cancellationToken = default);
    Task<IEnumerable<RoomPriceDto>> GetAllActivePricesAsync(CancellationToken cancellationToken = default);
    Task<RoomPriceDto> CreateRoomPriceAsync(CreateRoomPriceRequest request, CancellationToken cancellationToken = default);
    Task<RoomPriceDto?> UpdateRoomPriceAsync(long id, UpdateRoomPriceRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteRoomPriceAsync(long id, CancellationToken cancellationToken = default);
}
