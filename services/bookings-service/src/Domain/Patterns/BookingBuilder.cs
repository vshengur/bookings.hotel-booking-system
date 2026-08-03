using System;

using BookingService.Domain.Aggregates.Booking;

namespace BookingService.Domain.Patterns
{
    public class BookingBuilder
    {
        private Guid _id;
        private Guid _userId;
        private DateOnly _checkIn;
        private DateOnly _checkOut;

        public BookingBuilder WithId(Guid id) { _id = id; return this; }
        public BookingBuilder ForGuest(Guid userId) { _userId = userId; return this; }
        public BookingBuilder InPeriod(DateOnly checkIn, DateOnly checkOut)
        {
            _checkIn = checkIn;
            _checkOut = checkOut;
            return this;
        }

        public Booking Build() => new(_id, _userId, _checkIn, _checkOut);
    }
}
