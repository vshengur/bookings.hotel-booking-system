using Bookings.Common;
using Bookings.Common.Exceptions;

using BookingService.Domain.Events;
using BookingService.Domain.ValueObjects;

using System;

namespace BookingService.Domain.Entities;
public sealed class Booking : Entity, IAggregateRoot
{
    public enum BookingStatus { Created, PendingPayment, Confirmed, Cancelled }

    public Guid Id { get; private set; }
    public Guid RoomId { get; private set; }
    public Guid GuestId { get; private set; }
    public DateOnly CheckInDate { get; private set; }
    public DateOnly CheckOutDate { get; private set; }
    public Money TotalPrice { get; private set; }
    public BookingStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ConfirmedAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }

    private Booking() { }               // для ORM

    protected internal Booking(Guid id, Guid roomId, Guid guestId,
                    DateOnly checkIn, DateOnly checkOut,
                    Money totalPrice)
    {
        Id = id;
        RoomId = roomId;
        GuestId = guestId;
        CheckInDate = checkIn;
        CheckOutDate = checkOut;
        TotalPrice = totalPrice;
        Status = BookingStatus.Created;
        CreatedAtUtc = DateTime.UtcNow;

        AddEvent(new BookingCreatedDomainEvent(Id, RoomId, GuestId, TotalPrice));
    }

    // Factory-метод
    public static Booking Create(Guid roomId, Guid guestId,
                                 DateOnly checkIn, DateOnly checkOut,
                                 Money totalPrice)
    {
        if (checkIn >= checkOut)
            throw new BusinessRuleException("Check-out must be after check-in");

        return new Booking(Guid.NewGuid(), roomId, guestId, checkIn, checkOut, totalPrice);
    }

    public void MarkPendingPayment()
    {
        EnsureState(BookingStatus.Created);
        Status = BookingStatus.PendingPayment;
        AddEvent(new BookingPendingPaymentDomainEvent(Id));
    }

    public void Confirm()
    {
        EnsureState(BookingStatus.PendingPayment);
        Status = BookingStatus.Confirmed;
        ConfirmedAtUtc = DateTime.UtcNow;
        AddEvent(new BookingConfirmedDomainEvent(Id));
    }

    public void Cancel(string reason)
    {
        if (Status is BookingStatus.Cancelled or BookingStatus.Confirmed)
            throw new BusinessRuleException("Booking is already finalised");

        Status = BookingStatus.Cancelled;
        CancelledAtUtc = DateTime.UtcNow;
        AddEvent(new BookingCancelledDomainEvent(Id, reason));
    }

    private void EnsureState(BookingStatus required)
    {
        if (Status != required)
            throw new BusinessRuleException($"Booking must be {required} to perform this operation.");
    }
}