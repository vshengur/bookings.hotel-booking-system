using System;

using BookingService.Domain.Aggregates.Booking;

namespace BookingService.Application.DTOs;

public record BookingDto
{
    public Guid Id { get; init; }
    public long RoomId { get; init; }
    public Guid GuestId { get; init; }
    public DateOnly CheckIn { get; init; }
    public DateOnly CheckOut { get; init; }
    public string TotalPrice { get; init; } = default!;
    public BookingStatus Status { get; init; }
}
