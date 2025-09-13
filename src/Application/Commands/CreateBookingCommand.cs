using Bookings.Common.ValueObjects;

using BookingService.Domain.Aggregates.Booking;

using MediatR;

using System;
using System.Collections.Generic;

namespace BookingService.Application.Commands;

public record CreateBookingCommand(
    Guid BookingId,
    Guid GuestId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    IReadOnlyCollection<BookingLineItem> Items,
    string? PromoCode) : IRequest;
