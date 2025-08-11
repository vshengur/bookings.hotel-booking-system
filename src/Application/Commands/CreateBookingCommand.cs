using System;

namespace BookingService.Application.Commands;

public record CreateBookingCommand(Guid RoomId, Guid UserId, DateOnly CheckIn, DateOnly CheckOut, decimal BasePrice, string? PromoCode);
