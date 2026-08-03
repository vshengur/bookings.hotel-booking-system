using PricingService.Domain.Entities;

namespace PricingService.Domain.Interfaces;

/// <summary>
/// Repository interface for RoomPrice entity
/// </summary>
public interface IRoomPriceRepository
{
    Task<RoomPrice?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<RoomPrice?> GetByRoomIdAsync(long roomId, CancellationToken cancellationToken = default);
    Task<RoomPrice?> GetActiveByRoomTypeAsync(string roomType, DateTime date, CancellationToken cancellationToken = default);
    Task<IEnumerable<RoomPrice>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<RoomPrice>> GetByRoomTypeAsync(string roomType, CancellationToken cancellationToken = default);
    Task<RoomPrice> AddAsync(RoomPrice roomPrice, CancellationToken cancellationToken = default);
    Task UpdateAsync(RoomPrice roomPrice, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
