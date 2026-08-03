using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingService.Domain.Aggregates.Booking;

public enum BookingStatus
{
    Created = 0,
    Pending = 1,
    AwaitingPayment = 2,
    Reserved = 3,
    Confirmed = 4,
    Cancelled = 5,
    Failed = 6,
    Expired = 7
}
