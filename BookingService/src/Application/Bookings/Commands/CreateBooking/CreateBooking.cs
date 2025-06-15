using BookingService.Application.Common.Interfaces;
using BookingService.Domain.Entities;
using BookingService.Domain.Events;

namespace BookingService.Application.Bookings.Commands.CreateBooking;

public record CreateBookingCommand : IRequest<int>
{
    public int RoomId { get; init; }
    public DateTime CheckInDate { get; init; }
    public DateTime CheckOutDate { get; init; }
    public string GuestName { get; init; }
    public string GuestEmail { get; init; }
    public string? SpecialRequests { get; init; }
}

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateBookingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var entity = new Booking
        {
            RoomId = request.RoomId,
            CheckInDate = request.CheckInDate,
            CheckOutDate = request.CheckOutDate,
            GuestName = request.GuestName,
            GuestEmail = request.GuestEmail,
            SpecialRequests = request.SpecialRequests,
            Status = BookingStatus.Pending
        };

        entity.AddDomainEvent(new BookingCreatedEvent(entity));

        _context.Bookings.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
} 