using BookingService.Domain.Entities;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Application.Interfaces
{
    public interface IBookingRepository
    {
        Task<Booking?> GetAsync(Guid id);
        Task AddAsync(Booking booking);
        Task<int> GetOccupancyPercentAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    }
}
