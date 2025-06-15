namespace BookingService.Domain.Entities;

internal class Booking : BaseAuditableEntity
{
    public int HotelId { get; set; }

    public string? Note { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int Rooms { get; set; }

    public required string[] Hosts { get; set; }
}
