using System;

using Bookings.Common.ValueObjects;

using BookingService.Domain.Entities;

namespace BookingService.Domain.Patterns
{
    public class BookingBuilder
    {
        private Guid _id = Guid.NewGuid();
        private Guid _roomId;
        private Guid _userId;
        private DateOnly _checkIn;
        private DateOnly _checkOut;
        private Money _price;

        public BookingBuilder WithRoom(Guid roomId) { _roomId = roomId; return this; }
        public BookingBuilder WithUser(Guid userId) { _userId = userId; return this; }
        public BookingBuilder Between(DateOnly checkIn, DateOnly checkOut) { _checkIn = checkIn; _checkOut = checkOut; return this; }
        public BookingBuilder Price(decimal price) { _price = new Money(price); return this; }

        public Booking Build() => new(_id, _roomId, _userId, _checkIn, _checkOut, _price);
    }
}
