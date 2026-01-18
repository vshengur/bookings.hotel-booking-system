using Microsoft.EntityFrameworkCore;
using PricingService.Domain.Entities;
using PricingService.Domain.Interfaces;
using PricingService.Infrastructure.Data;

namespace PricingService.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for RoomPrice entity
/// </summary>
public class RoomPriceRepository : IRoomPriceRepository
{
    private readonly PricingDbContext _context;

    public RoomPriceRepository(PricingDbContext context)
    {
        _context = context;
    }

    public async Task<RoomPrice?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.RoomPrices
            .FirstOrDefaultAsync(rp => rp.Id == id, cancellationToken);
    }

    public async Task<RoomPrice?> GetByRoomIdAsync(long roomId, CancellationToken cancellationToken = default)
    {
        return await _context.RoomPrices
            .Where(rp => rp.RoomId == roomId && rp.IsActive)
            .OrderByDescending(rp => rp.ValidFrom)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<RoomPrice?> GetActiveByRoomTypeAsync(string roomType, DateTime date, CancellationToken cancellationToken = default)
    {
        return await _context.RoomPrices
            .Where(rp => rp.RoomType == roomType
                && rp.IsActive
                && rp.ValidFrom <= date
                && (rp.ValidTo == null || rp.ValidTo >= date))
            .OrderByDescending(rp => rp.ValidFrom)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<RoomPrice>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.RoomPrices
            .Where(rp => rp.IsActive)
            .OrderBy(rp => rp.RoomType)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RoomPrice>> GetByRoomTypeAsync(string roomType, CancellationToken cancellationToken = default)
    {
        return await _context.RoomPrices
            .Where(rp => rp.RoomType == roomType)
            .OrderByDescending(rp => rp.ValidFrom)
            .ToListAsync(cancellationToken);
    }

    public async Task<RoomPrice> AddAsync(RoomPrice roomPrice, CancellationToken cancellationToken = default)
    {
        await _context.RoomPrices.AddAsync(roomPrice, cancellationToken);
        return roomPrice;
    }

    public Task UpdateAsync(RoomPrice roomPrice, CancellationToken cancellationToken = default)
    {
        _context.RoomPrices.Update(roomPrice);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var roomPrice = await GetByIdAsync(id, cancellationToken);
        if (roomPrice != null)
        {
            _context.RoomPrices.Remove(roomPrice);
        }
    }
}
