using Bookings.Common;
using System;

namespace BookingService.Domain.Events;

public record BookingCreatedDomainEvent(Guid BookingId, Guid GuestId, DateOnly CheckIn, DateOnly CheckOut) : IDomainEvent;
public record BookingPendingPaymentDomainEvent(Guid BookingId) : IDomainEvent;
public record BookingConfirmedDomainEvent(Guid BookingId) : IDomainEvent;
public record BookingCancelledDomainEvent(Guid BookingId, string Reason) : IDomainEvent;