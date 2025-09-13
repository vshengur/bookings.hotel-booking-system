using BookingService.Domain.Aggregates.Booking;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Application.Interfaces
{
    public interface IBookingRepository
    {
        Task<Booking?> GetAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(Booking booking, CancellationToken ct = default);
        Task UpsertAsync(Booking booking, CancellationToken ct = default);
        Task<int> GetOccupancyPercentAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    }
}
