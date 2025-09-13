using MediatR;

using System;

namespace BookingService.Application.Commands;

public record CreatePaymentCommand(Guid BookingId) : IRequest;
