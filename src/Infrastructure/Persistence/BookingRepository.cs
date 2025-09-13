using BookingService.Application.Interfaces;
using BookingService.Domain.Aggregates.Booking;

using Microsoft.EntityFrameworkCore;

using Polly;
using Polly.Retry;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Infrastructure.Persistence
{
    public class BookingRepository : IBookingRepository
    {
        private readonly BookingDbContext _context;
        private readonly AsyncRetryPolicy _retryPolicy;

        public BookingRepository(BookingDbContext context)
        {
            _context = context;
            _retryPolicy = Policy
                .Handle<DbUpdateException>()
                .WaitAndRetryAsync(3,
                    attempt => TimeSpan.FromMilliseconds(200 * attempt));
        }

        public async Task AddAsync(Booking booking, CancellationToken ct = default)
        {
            await _retryPolicy.ExecuteAsync(() =>
                _context.Bookings.AddAsync(booking, ct).AsTask());
        }

        public Task<Booking?> GetAsync(Guid id, CancellationToken ct = default) =>
            _context.Bookings.FirstOrDefaultAsync(b => b.Id == id, ct);
        
        public async Task<int> GetOccupancyPercentAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
        {
            // ⚠️  Заглушка: в реальном проекте нужно брать «кол-во занятых комнат / общее число комнат».
            // Пока просто считаем кол-во бронирований в нужном диапазоне
            var total = await _context.Bookings.CountAsync(ct);
            if (total == 0) return 0;

            var overlapped = await _context.Bookings
                .CountAsync(b =>
                      b.CheckInDate < to &&
                      b.CheckOutDate > from, ct);

            return (int)Math.Round((double)overlapped * 100 / total);
        }

        public Task UpsertAsync(Booking booking, CancellationToken ct = default(CancellationToken))
        {
            _context.Bookings.Update(booking);
            return Task.CompletedTask;
        }
    }
}
