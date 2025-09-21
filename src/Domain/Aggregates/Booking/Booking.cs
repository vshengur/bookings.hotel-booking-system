using Bookings.Common;
using Bookings.Common.Exceptions;
using Bookings.Common.ValueObjects;

using BookingService.Domain.Events;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BookingService.Domain.Aggregates.Booking;

public sealed class Booking : Entity, IAggregateRoot
{
    private readonly List<BookingLineItem> _items = [];

    public Guid Id { get; private set; }
    public Guid GuestId { get; private set; }
    public DateOnly CheckInDate { get; private set; }
    public DateOnly CheckOutDate { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ConfirmedAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public IReadOnlyCollection<BookingLineItem> Items => new ReadOnlyCollection<BookingLineItem>(_items);
    public Money TotalPrice => _items.Aggregate(Money.Zero(), (acc, li) => acc + li.PricePerNight.Multiply(li.Nights));

    private Booking() { } // для ORM

    protected internal Booking(Guid id, Guid guestId, DateOnly checkIn, DateOnly checkOut)
        :base()
    {
        Id = id;
        GuestId = guestId;
        CheckInDate = checkIn;
        CheckOutDate = checkOut;
        Status = BookingStatus.Created;
        CreatedAtUtc = DateTime.UtcNow;

        AddEvent(new BookingCreatedDomainEvent(Id, GuestId, CheckInDate, CheckOutDate));
    }

    // Factory-метод
    public static Booking Create(Guid bookingId, Guid guestId, DateOnly checkIn, DateOnly checkOut)
    {
        if (checkIn >= checkOut)
            throw new BusinessRuleException("Check-out must be after check-in");
        if (bookingId == Guid.Empty) throw new BusinessRuleException("Booking id required");
        if (guestId == Guid.Empty) throw new BusinessRuleException("Guest id required");

        return new Booking(bookingId, guestId, checkIn, checkOut);
    }

    public void AddLineItem(Guid roomId, int adults, int children, int nights, Money pricePerNight)
    {
        if (roomId == Guid.Empty) throw new ArgumentException("RoomId required");

        ArgumentOutOfRangeException.ThrowIfNegative(adults);
        ArgumentOutOfRangeException.ThrowIfNegative(children);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(nights);

        _items.Add(new BookingLineItem(Id, roomId, adults, children, nights, pricePerNight));
    }

    public void MarkAwaitingPayment()
    {
        EnsureState(BookingStatus.Created);
        Status = BookingStatus.AwaitingPayment;
        AddEvent(new BookingPendingPaymentDomainEvent(Id));
    }

    public void MarkReserved()
    {
        EnsureState(BookingStatus.AwaitingPayment);
        Status = BookingStatus.Reserved;
        AddEvent(new BookingReservedDomainEvent(Id));
    }

    public void MarkConfirmed()
    {
        EnsureState(BookingStatus.Reserved);
        Status = BookingStatus.Confirmed;
        AddEvent(new BookingConfirmedDomainEvent(Id));
    }

    public void MarkCancelled(string reason)
    {
        if (string.IsNullOrEmpty(reason))
        {
            throw new BusinessRuleException($"Status '{BookingStatus.Cancelled}' should have cancel reason");
        }

        if (Status is BookingStatus.Cancelled or BookingStatus.Confirmed)
            throw new BusinessRuleException("Booking is already finalised");

        Status = BookingStatus.Cancelled;
        CancelledAtUtc = DateTime.UtcNow;
        AddEvent(new BookingCancelledDomainEvent(Id, reason));
    }

    public void MarkFailed()
    {
        Status = BookingStatus.Failed;
        AddEvent(new BookingFailedDomainEvent(Id));
    }

    public void MarkExpired()
    {
        Status = BookingStatus.Expired;
        AddEvent(new BookingExpiredDomainEvent(Id));
    }

    private void EnsureState(BookingStatus required)
    {
        if (Status != required)
            throw new BusinessRuleException($"Booking must be {required} to perform this operation.");
    }
}