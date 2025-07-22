using System;

namespace BookingService.Application.DTOs
{
    public record BookingDto
    {
        public Guid Id { get; init; }
        public Guid RoomId { get; init; }
        public Guid UserId { get; init; }
        public DateOnly CheckIn { get; init; }
        public DateOnly CheckOut { get; init; }
        public decimal TotalPrice { get; init; }
        public string Status { get; init; } = default!;
    }
}
