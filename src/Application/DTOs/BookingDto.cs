using System;

namespace BookingService.Application.DTOs
{
    public record BookingDto
    {
        public Guid Id { get; init; }
        public Guid RoomId { get; init; }
        public Guid GuestId { get; init; }
        public DateOnly CheckIn { get; init; }
        public DateOnly CheckOut { get; init; }
        public string TotalPrice { get; init; }
        public string Status { get; init; } = default!;
    }
}
