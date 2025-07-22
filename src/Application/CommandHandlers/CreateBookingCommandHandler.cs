using BookingService.Application.Commands;
using BookingService.Application.Interfaces;
using BookingService.Application.Pricing;
using BookingService.Domain.Patterns;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Application.CommandHandlers
{
    public class CreateBookingCommandHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly IPricingStrategyProvider _provider;

        public CreateBookingCommandHandler(
            IUnitOfWork uow,
            IPricingStrategyProvider provider)
        {
            _uow = uow;
            _provider = provider;
        }

        public async Task<Guid> Handle(CreateBookingCommand cmd, CancellationToken ct = default)
        {
            var occupancyPercent = await _uow.Bookings.GetOccupancyPercentAsync(cmd.CheckIn, cmd.CheckOut, ct);

            var ctx = new PricingContext
            {
                CheckIn = cmd.CheckIn,
                CheckOut = cmd.CheckOut,
                PromoCode = cmd.PromoCode,
                OccupancyPercent = occupancyPercent
            };

            var strategy = await _provider.Resolve(ctx);
            int nights = cmd.CheckOut.DayNumber - cmd.CheckIn.DayNumber;
            var total = strategy.Calculate(cmd.BasePrice, nights);

            var booking = new BookingBuilder()
                .WithRoom(cmd.RoomId)
                .WithUser(cmd.UserId)
                .Between(cmd.CheckIn, cmd.CheckOut)
                .Price(total)
                .Build();

            await _uow.Bookings.AddAsync(booking);
            await _uow.SaveChangesAsync();

            return booking.Id;
        }
    }
}
