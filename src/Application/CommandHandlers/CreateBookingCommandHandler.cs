using BookingService.Application.Commands;
using BookingService.Application.Interfaces;
using BookingService.Domain.Patterns;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Application.CommandHandlers;

public class CreateBookingCommandHandler
{
    private readonly IUnitOfWork _uow;

    public CreateBookingCommandHandler(
        IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Guid> Handle(CreateBookingCommand cmd, CancellationToken ct = default)
    {
        var booking = new BookingBuilder()
            .WithRoom(cmd.RoomId)
            .WithUser(cmd.UserId)
            .Between(cmd.CheckIn, cmd.CheckOut)
            .Price(total)
            .Build();

        await _uow.Bookings.AddAsync(booking);
        await _uow.SaveChangesAsync(ct);

        return booking.Id;
    }
}
