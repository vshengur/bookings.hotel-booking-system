using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingService.Infrastructure.Persistence.Models;

public class BookingEntity
{
    public Guid Id { get; set; }
    public Guid GuestId { get; set; }
    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
    public int Status { get; set; }
    public string? PaymentRef { get; set; }
    public string? PmsNumber { get; set; }
    public MoneyEmbeddable TotalPrice { get; set; } = new();
    public List<BookingLineItemEntity> Items { get; set; } = new();
}
public class BookingLineItemEntity
{
    public long Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid RoomId { get; set; }
    public int Adults { get; set; }
    public int Children { get; set; }
    public int Nights { get; set; }
    public MoneyEmbeddable PricePerNight { get; set; } = new();
}
[Owned] public class MoneyEmbeddable { public decimal Amount { get; set; } public string Currency { get; set; } = "EUR"; }
