using MediatR;

using System;

namespace BookingService.Application.Commands;

public record ReleaseInventoryCommand(Guid BookingId) : IRequest;
