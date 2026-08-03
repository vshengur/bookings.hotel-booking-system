using MediatR;

using System;

namespace BookingService.Application.Commands;

public record ConfirmBookingCommand(Guid BookingId) : IRequest;
