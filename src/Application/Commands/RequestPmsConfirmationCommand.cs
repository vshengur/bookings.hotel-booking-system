using MediatR;

using System;

namespace BookingService.Application.Commands;

public record RequestPmsConfirmationCommand(Guid BookingId) : IRequest;