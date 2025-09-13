using MediatR;

using System;

namespace BookingService.Application.Commands;

public record ReserveInventoryCommand(Guid BookingId) : IRequest;