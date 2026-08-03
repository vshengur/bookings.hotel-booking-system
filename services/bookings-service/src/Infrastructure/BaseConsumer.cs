using MassTransit;

using System.Threading.Tasks;

namespace BookingService.Infrastructure;

public abstract class BaseConsumer<T> : IConsumer<T> where T : class
{
    public abstract Task Consume(ConsumeContext<T> context);
}
