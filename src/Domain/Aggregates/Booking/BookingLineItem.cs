using Bookings.Common;
using Bookings.Common.ValueObjects;

using System;

namespace BookingService.Domain.Aggregates.Booking;

public sealed class BookingLineItem : Entity
{
    public BookingLineItem(Guid bookingId, Guid roomId, int adults, int children, int nights, Money pricePerNight)
    {
        Id = Guid.NewGuid();
        BookingId = bookingId;
        RoomId = roomId;
        Adults = adults;
        Children = children;
        Nights = nights;
        PricePerNight = pricePerNight;
    }

    private BookingLineItem() { }  // для ORM

    public Guid Id { get; private set; }
    public Guid BookingId { get; set; }
    public Guid RoomId { get; private set; }
    public int Adults { get; private set; }
    public int Children { get; private set; }
    public int Nights { get; private set; }
    public Money PricePerNight { get; private set; }
    public decimal SubtotalAmount => PricePerNight.Amount * Nights;
}
