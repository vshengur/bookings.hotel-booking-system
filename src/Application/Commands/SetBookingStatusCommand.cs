using BookingService.Domain.Aggregates.Booking;

using MediatR;

using System;

namespace BookingService.Application.Commands;

public record SetBookingStatusCommand(Guid BookingId, BookingStatus Status, string? CancelReason = null) : IRequest;