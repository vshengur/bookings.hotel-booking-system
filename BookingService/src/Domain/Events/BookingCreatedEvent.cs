using BookingService.Domain.Common;

namespace BookingService.Domain.Events;

public class BookingCreatedEvent : DomainEvent
{
    public BookingCreatedEvent(Booking booking)
    {
        Booking = booking;
    }

    public Booking Booking { get; }
} 