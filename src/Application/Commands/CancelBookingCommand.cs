using System;

namespace BookingService.Application.Commands;

public record CancelBookingCommand(Guid BookingId, string Reason);
