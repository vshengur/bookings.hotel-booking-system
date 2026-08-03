using System;
using System.Threading.Tasks;

using BookingService.Application.Commands;

using MassTransit;

namespace BookingService.Infrastructure.Jobs
{
    public class CancelUnpaidBookingJob
    {
        private readonly IPublishEndpoint _publish;

        public CancelUnpaidBookingJob(IPublishEndpoint publish)
        {
            _publish = publish;
        }

        public Task Execute(Guid bookingId, string reason) =>
            _publish.Publish(new CancelBookingCommand(bookingId, reason));
    }
}
