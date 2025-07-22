using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingService.Application.Pricing;

public record PricingContext
{
    public DateOnly CheckIn { get; init; }
    public DateOnly CheckOut { get; init; }
    public string? PromoCode { get; init; }
    public int OccupancyPercent { get; init; }
}
