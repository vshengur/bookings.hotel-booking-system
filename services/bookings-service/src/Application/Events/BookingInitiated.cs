using MediatR;

using System;

namespace BookingService.Application.Events;

/// <summary>
/// Internal application event (domain/app event). Infrastructure translates it to bus contract CreateBooking.
/// </summary>
public record BookingInitiated(Guid BookingId) : INotification;