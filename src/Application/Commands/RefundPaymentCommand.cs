using MediatR;

using System;

namespace BookingService.Application.Commands;

public record RefundPaymentCommand(Guid BookingId) : IRequest;