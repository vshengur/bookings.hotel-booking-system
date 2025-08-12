using Bookings.Common;
using Bookings.Common.ValueObjects;

using System;

namespace BookingService.Domain.Events;

public record BookingCreatedDomainEvent(Guid BookingId, Guid RoomId, Guid GuestId, Money TotalPrice) : IDomainEvent;
public record BookingPendingPaymentDomainEvent(Guid BookingId) : IDomainEvent;
public record BookingConfirmedDomainEvent(Guid BookingId) : IDomainEvent;
public record BookingCancelledDomainEvent(Guid BookingId, string Reason) : IDomainEvent;